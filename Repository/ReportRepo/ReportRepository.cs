using InventoryManagementSystem.DAL;
using InventoryManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Repository.ReportRepo
{
    public class ReportRepository : IReportRepository
    {
        private readonly InventoryDbContext _context;

        public ReportRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<ReportVM> GetReportDataAsync(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var salesQuery = _context.Sales.AsQueryable();

            var purchasesQuery = _context.Purchases.AsQueryable();

            var stockMovementsQuery =
                _context.StockMovements
                    .Include(sm => sm.Product)
                    .AsQueryable();


            // From Date Filter
            if (fromDate.HasValue)
            {
                salesQuery = salesQuery.Where(
                    s => s.SaleDate >= fromDate.Value);

                purchasesQuery = purchasesQuery.Where(
                    p => p.PurchaseDate >= fromDate.Value);

                stockMovementsQuery = stockMovementsQuery.Where(
                    sm => sm.MovementDate >= fromDate.Value);
            }


            // To Date Filter
            if (toDate.HasValue)
            {
                var nextDay = toDate.Value.Date.AddDays(1);

                salesQuery = salesQuery.Where(
                    s => s.SaleDate < nextDay);

                purchasesQuery = purchasesQuery.Where(
                    p => p.PurchaseDate < nextDay);

                stockMovementsQuery = stockMovementsQuery.Where(
                    sm => sm.MovementDate < nextDay);
            }


            var report = new ReportVM
            {
                FromDate = fromDate,

                ToDate = toDate,


                // Sales
                Sales = await salesQuery
                    .OrderByDescending(s => s.SaleDate)
                    .ToListAsync(),


                // Purchases
                Purchases = await purchasesQuery
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync(),


                // All Products
                Products = await _context.Products
                    .OrderBy(p => p.ProductName)
                    .ToListAsync(),


                // Low Stock Products
                LowStockProducts = await _context.Products
                    .Where(p =>
                        p.Quantity <= p.ReorderLevel)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync(),


                // Stock Movements
                StockMovements = await stockMovementsQuery
                    .OrderByDescending(sm => sm.MovementDate)
                    .ToListAsync()
            };


            return report;
        }
    }
}