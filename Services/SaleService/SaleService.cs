
using InventoryManagementSystem.DAL;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Repository.ProductRepo;
using InventoryManagementSystem.Repository.SaleRepo;
using InventoryManagementSystem.Repository.StockMovementRepo;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.SaleService
{
    public class SaleService : ISaleService
    {
        private readonly InventoryDbContext _context;
        private readonly ISaleRepository _saleRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockMovementRepository;

        public SaleService(
            InventoryDbContext context,
            ISaleRepository saleRepository,
            IProductRepository productRepository,
            IStockMovementRepository stockMovementRepository)
        {
            _context = context;
            _saleRepository = saleRepository;
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
        }

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<Sale>> GetAllAsync()
        {
            return await _saleRepository.GetAllAsync();
        }

        // =========================
        // GET BY ID
        // =========================

        public async Task<Sale> GetByIdAsync(int id)
        {
            return await _saleRepository.GetByIdAsync(id);
        }

        // =========================
        // ADD SALE
        // =========================

        public async Task AddAsync(Sale sale)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                decimal totalAmount = 0;

                // Validate sale items
                if (sale.SaleItems == null ||
                    !sale.SaleItems.Any())
                {
                    throw new InvalidOperationException(
                        "Please add at least one product.");
                }

                // Check the combined quantity for duplicate product lines.
                var requestedByProduct = sale.SaleItems
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                foreach (var requested in requestedByProduct)
                {
                    var stockProduct = await _productRepository.GetByIdAsync(requested.Key);
                    if (stockProduct == null)
                    {
                        throw new InvalidOperationException(
                            $"Product with ID {requested.Key} not found.");
                    }

                    if (requested.Value > stockProduct.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Stock is less for {stockProduct.ProductName}. " +
                            $"Available stock: {stockProduct.Quantity}, requested: {requested.Value}.");
                    }
                }

                foreach (var item in sale.SaleItems)
                {
                    // Quantity validation
                    if (item.Quantity <= 0)
                    {
                        throw new InvalidOperationException(
                            "Quantity must be greater than 0.");
                    }

                    // Get product
                    var product =
                        await _productRepository.GetByIdAsync(
                            item.ProductId);

                    if (product == null)
                    {
                        throw new InvalidOperationException(
                            $"Product with ID {item.ProductId} not found.");
                    }

                    // Stock validation
                    if (product.Quantity < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for {product.ProductName}. " +
                            $"Available stock: {product.Quantity}");
                    }

                    // Product master selling price
                    item.UnitPrice = product.SellingPrice;

                    // Calculate item total
                    item.TotalPrice =
                        item.Quantity * item.UnitPrice;

                    totalAmount += item.TotalPrice;
                }

                // Calculate sale total
                sale.TotalAmount = totalAmount;

                // Save sale first so SQL Server assigns SaleId.
                await _saleRepository.AddAsync(sale);

                // Every completed sale automatically creates an outstanding invoice.
                // This keeps Billing totals in sync without requiring a second manual step.
                var existingBilling = await _context.Billings
                    .FirstOrDefaultAsync(b => b.SaleId == sale.SaleId);

                if (existingBilling == null)
                {
                    var tax = Math.Round(totalAmount * 0.18m, 2);

                    var billing = new Billing
                    {
                        SaleId = sale.SaleId,
                        InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{sale.SaleId:D5}",
                        IssueDate = DateTime.Now,
                        CustomerName = "Walk-in Customer",
                        Subtotal = totalAmount,
                        TaxRate = 18m,
                        TaxAmount = tax,
                        Discount = 0m,
                        GrandTotal = totalAmount + tax,
                        Status = "Outstanding"
                    };

                    _context.Billings.Add(billing);
                    await _context.SaveChangesAsync();

                    // Create a pending payment record immediately. It becomes Paid
                    // when the seller records the actual payment.
                    _context.Payments.Add(new Payment
                    {
                        BillingId = billing.BillingId,
                        Amount = billing.GrandTotal,
                        Method = "Pending",
                        Status = "Pending",
                        TransactionReference = $"PENDING-{billing.InvoiceNumber}",
                        PaidAt = billing.IssueDate
                    });

                    await _context.SaveChangesAsync();
                }

                // Decrease stock + create movement
                foreach (var item in sale.SaleItems)
                {
                    var product =
                        await _productRepository.GetByIdAsync(
                            item.ProductId);

                    if (product == null)
                    {
                        throw new InvalidOperationException(
                            $"Product with ID {item.ProductId} not found.");
                    }

                    product.Quantity -= item.Quantity;

                    await _productRepository.UpdateAsync(product);

                    var movement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        MovementType = "OUT",
                        MovementDate = sale.SaleDate,
                        Reference = "Sale #" + sale.SaleId
                    };

                    await _stockMovementRepository.AddAsync(
                        movement);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================
        // UPDATE SALE
        // =========================

        public async Task UpdateAsync(Sale sale)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var existingSale =
                    await _saleRepository.GetByIdAsync(
                        sale.SaleId);

                if (existingSale == null)
                {
                    throw new InvalidOperationException(
                        "Sale not found.");
                }

                // Restore stock from old sale
                foreach (var oldItem in existingSale.SaleItems)
                {
                    var product =
                        await _productRepository.GetByIdAsync(
                            oldItem.ProductId);

                    if (product != null)
                    {
                        product.Quantity += oldItem.Quantity;

                        await _productRepository.UpdateAsync(
                            product);
                    }
                }

                // Validate new sale items
                if (sale.SaleItems == null ||
                    !sale.SaleItems.Any())
                {
                    throw new InvalidOperationException(
                        "Please add at least one product.");
                }

                // Validate the total requested quantity per product so multiple
                // lines for the same product cannot oversell available stock.
                var requestedByProduct = sale.SaleItems
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                foreach (var requested in requestedByProduct)
                {
                    var stockProduct = await _productRepository.GetByIdAsync(requested.Key);
                    if (stockProduct == null)
                    {
                        throw new InvalidOperationException(
                            $"Product with ID {requested.Key} not found.");
                    }

                    if (requested.Value > stockProduct.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Stock is less for {stockProduct.ProductName}. " +
                            $"Available stock: {stockProduct.Quantity}, requested: {requested.Value}.");
                    }
                }

                decimal totalAmount = 0;

                foreach (var item in sale.SaleItems)
                {
                    if (item.Quantity <= 0)
                    {
                        throw new InvalidOperationException(
                            "Quantity must be greater than 0.");
                    }

                    var product =
                        await _productRepository.GetByIdAsync(
                            item.ProductId);

                    if (product == null)
                    {
                        throw new InvalidOperationException(
                            $"Product with ID {item.ProductId} not found.");
                    }

                    // Check available stock
                    if (product.Quantity < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for {product.ProductName}. " +
                            $"Available stock: {product.Quantity}");
                    }

                    // Product master selling price
                    item.UnitPrice = product.SellingPrice;

                    // Item total
                    item.TotalPrice =
                        item.Quantity * item.UnitPrice;

                    totalAmount += item.TotalPrice;
                }

                // Update sale
                sale.TotalAmount = totalAmount;

                // Remove old items
                _context.SaleItems.RemoveRange(
                    existingSale.SaleItems);

                await _context.SaveChangesAsync();

                existingSale.SaleDate = sale.SaleDate;
                existingSale.TotalAmount = sale.TotalAmount;

                // Add new items + decrease stock
                foreach (var item in sale.SaleItems)
                {
                    item.SaleId = existingSale.SaleId;

                    var product =
                        await _productRepository.GetByIdAsync(
                            item.ProductId);

                    if (product == null)
                    {
                        throw new InvalidOperationException(
                            $"Product with ID {item.ProductId} not found.");
                    }

                    product.Quantity -= item.Quantity;

                    await _productRepository.UpdateAsync(
                        product);

                    _context.SaleItems.Add(item);

                    var movement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        MovementType = "OUT",
                        MovementDate = sale.SaleDate,
                        Reference =
                            "Sale Update #" + sale.SaleId
                    };

                    await _stockMovementRepository.AddAsync(
                        movement);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================
        // DELETE SALE
        // =========================

        public async Task DeleteAsync(int id)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var sale =
                    await _saleRepository.GetByIdAsync(id);

                if (sale == null)
                {
                    throw new InvalidOperationException(
                        "Sale not found.");
                }

                // Restore stock
                foreach (var item in sale.SaleItems)
                {
                    var product =
                        await _productRepository.GetByIdAsync(
                            item.ProductId);

                    if (product != null)
                    {
                        product.Quantity += item.Quantity;

                        await _productRepository.UpdateAsync(
                            product);
                    }

                    // Stock reversal movement
                    var movement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        MovementType = "IN",
                        MovementDate = DateTime.Now,
                        Reference = "Sale Delete #" + id
                    };

                    await _stockMovementRepository.AddAsync(
                        movement);
                }

                // Delete sale
                await _saleRepository.DeleteAsync(id);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
