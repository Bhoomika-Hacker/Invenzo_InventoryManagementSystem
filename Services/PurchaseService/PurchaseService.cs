
using InventoryManagementSystem.DAL;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Repository.ProductRepo;
using InventoryManagementSystem.Repository.PurchaseRepo;
using InventoryManagementSystem.Repository.StockMovementRepo;

namespace InventoryManagementSystem.Services.PurchaseService
{
    public class PurchaseService : IPurchaseService
    {
        private readonly InventoryDbContext _context;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockMovementRepository;

        public PurchaseService(
            InventoryDbContext context,
            IPurchaseRepository purchaseRepository,
            IProductRepository productRepository,
            IStockMovementRepository stockMovementRepository)
        {
            _context = context;
            _purchaseRepository = purchaseRepository;
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
        }


        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<Purchase>> GetAllAsync()
        {
            return await _purchaseRepository.GetAllAsync();
        }


        // =========================
        // GET BY ID
        // =========================

        public async Task<Purchase> GetByIdAsync(int id)
        {
            return await _purchaseRepository.GetByIdAsync(id);
        }


        // =========================
        // ADD PURCHASE
        // =========================

        public async Task AddAsync(Purchase purchase)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                decimal totalAmount = 0;


                // =========================
                // VALIDATE & CALCULATE
                // =========================

                foreach (var item in purchase.PurchaseItems)
                {
                    var product =
                        await _productRepository.GetByIdAsync(
                            item.ProductId);

                    if (product == null)
                    {
                        throw new InvalidOperationException(
                            $"Product with ID {item.ProductId} not found.");
                    }


                    if (item.Quantity < 0)
                    {
                        throw new InvalidOperationException(
                            "Purchase quantity must be greater than or equal to 0.");
                    }


                    // Use Product Purchase Price

                    item.UnitPrice =
                        product.PurchasePrice;


                    // Calculate item total

                    item.TotalPrice =
                        item.Quantity *
                        item.UnitPrice;


                    totalAmount +=
                        item.TotalPrice;
                }


                // =========================
                // SET PURCHASE TOTAL
                // =========================

                purchase.TotalAmount =
                    totalAmount;


                // =========================
                // SAVE PURCHASE
                // =========================

                await _purchaseRepository.AddAsync(
                    purchase);


                // =========================
                // UPDATE STOCK
                // =========================

                foreach (var item in purchase.PurchaseItems)
                {
                    var product =
                        await _productRepository.GetByIdAsync(
                            item.ProductId);

                    if (product == null)
                    {
                        throw new InvalidOperationException(
                            $"Product with ID {item.ProductId} not found.");
                    }


                    product.Quantity +=
                        item.Quantity;


                    await _productRepository.UpdateAsync(
                        product);


                    // =========================
                    // STOCK MOVEMENT
                    // =========================

                    var stockMovement =
                        new StockMovement
                        {
                            ProductId =
                                item.ProductId,

                            MovementType = "IN",

                            Quantity =
                                item.Quantity,

                            MovementDate =
                                purchase.PurchaseDate,

                            Reference =
                                "Purchase #" +
                                purchase.PurchaseId
                        };


                    await _stockMovementRepository.AddAsync(
                        stockMovement);
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
        // UPDATE PURCHASE
        // =========================

        public async Task UpdateAsync(Purchase purchase)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // =========================
                // GET EXISTING PURCHASE
                // =========================

                var existingPurchase =
                    await _purchaseRepository.GetByIdAsync(
                        purchase.PurchaseId);

                if (existingPurchase == null)
                {
                    throw new InvalidOperationException(
                        "Purchase not found.");
                }


                // =========================
                // REMOVE OLD STOCK
                // =========================

                foreach (var oldItem
                    in existingPurchase.PurchaseItems)
                {
                    var product =
                        await _productRepository.GetByIdAsync(
                            oldItem.ProductId);

                    if (product != null)
                    {
                        product.Quantity -=
                            oldItem.Quantity;

                        await _productRepository.UpdateAsync(
                            product);
                    }
                }


                // =========================
                // REMOVE OLD ITEMS
                // =========================

                _context.PurchaseItems.RemoveRange(
                    existingPurchase.PurchaseItems);

                await _context.SaveChangesAsync();


                // =========================
                // UPDATE PURCHASE DETAILS
                // =========================

                existingPurchase.SupplierId =
                    purchase.SupplierId;

                existingPurchase.PurchaseDate =
                    purchase.PurchaseDate;


                decimal totalAmount = 0;


                // =========================
                // ADD NEW ITEMS
                // =========================

                foreach (var item
                    in purchase.PurchaseItems)
                {
                    item.PurchaseId =
                        existingPurchase.PurchaseId;


                    // =========================
                    // GET PRODUCT
                    // =========================

                    var product =
                        await _productRepository.GetByIdAsync(
                            item.ProductId);

                    if (product == null)
                    {
                        throw new InvalidOperationException(
                            $"Product with ID {item.ProductId} not found.");
                    }


                    if (item.Quantity < 0)
                    {
                        throw new InvalidOperationException(
                            "Purchase quantity must be greater than or equal to 0.");
                    }


                    // =========================
                    // USE PURCHASE PRICE
                    // =========================

                    item.UnitPrice =
                        product.PurchasePrice;


                    // =========================
                    // ITEM TOTAL
                    // =========================

                    item.TotalPrice =
                        item.Quantity *
                        item.UnitPrice;


                    totalAmount +=
                        item.TotalPrice;


                    // =========================
                    // UPDATE STOCK
                    // =========================

                    product.Quantity +=
                        item.Quantity;

                    await _productRepository.UpdateAsync(
                        product);


                    // =========================
                    // ADD PURCHASE ITEM
                    // =========================

                    _context.PurchaseItems.Add(item);


                    // =========================
                    // STOCK MOVEMENT
                    // =========================

                    var stockMovement =
                        new StockMovement
                        {
                            ProductId =
                                item.ProductId,

                            MovementType = "IN",

                            Quantity =
                                item.Quantity,

                            MovementDate =
                                purchase.PurchaseDate,

                            Reference =
                                "Purchase Edit #" +
                                purchase.PurchaseId
                        };


                    await _stockMovementRepository.AddAsync(
                        stockMovement);
                }


                // =========================
                // UPDATE TOTAL
                // =========================

                existingPurchase.TotalAmount =
                    totalAmount;


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
        // DELETE PURCHASE
        // =========================

        public async Task DeleteAsync(int id)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // =========================
                // GET PURCHASE
                // =========================

                var purchase =
                    await _purchaseRepository.GetByIdAsync(id);

                if (purchase == null)
                {
                    await transaction.CommitAsync();
                    return;
                }


                // =========================
                // REMOVE PURCHASE STOCK
                // =========================

                foreach (var item
                    in purchase.PurchaseItems)
                {
                    var product =
                        await _productRepository.GetByIdAsync(
                            item.ProductId);

                    if (product != null)
                    {
                        product.Quantity -=
                            item.Quantity;

                        await _productRepository.UpdateAsync(
                            product);


                        // =========================
                        // STOCK MOVEMENT
                        // =========================

                        var stockMovement =
                            new StockMovement
                            {
                                ProductId =
                                    item.ProductId,

                                MovementType = "OUT",

                                Quantity =
                                    item.Quantity,

                                MovementDate =
                                    DateTime.Now,

                                Reference =
                                    "Purchase Delete #" +
                                    purchase.PurchaseId
                            };


                        await _stockMovementRepository.AddAsync(
                            stockMovement);
                    }
                }


                // =========================
                // DELETE PURCHASE ITEMS
                // =========================

                _context.PurchaseItems.RemoveRange(
                    purchase.PurchaseItems);


                // =========================
                // DELETE PURCHASE
                // =========================

                _context.Purchases.Remove(
                    purchase);


                await _context.SaveChangesAsync();

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

