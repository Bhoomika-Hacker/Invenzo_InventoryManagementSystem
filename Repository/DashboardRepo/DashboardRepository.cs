using InventoryManagementSystem.DAL;
using InventoryManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Repository.DashboardRepo
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly InventoryDbContext _context;

        public DashboardRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardVM> GetDashboardDataAsync()
        {
            var now = DateTime.Now;
            var currentMonth = new DateTime(now.Year, now.Month, 1);
            var sixMonthStart = currentMonth.AddMonths(-5);

            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            var sales = await _context.Sales
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            var recentPurchases = await _context.Purchases
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .Take(5)
                .ToListAsync();

            var recentMovements = await _context.StockMovements
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.MovementDate)
                .ThenByDescending(sm => sm.StockMovementId)
                .Take(10)
                .ToListAsync();

            var totalProducts = products.Count;
            var lowStock = products.Where(p => p.Quantity <= p.ReorderLevel && p.Quantity > 0).ToList();
            var outOfStock = products.Where(p => p.Quantity <= 0).ToList();
            var inStock = products.Where(p => p.Quantity > p.ReorderLevel).ToList();

            var monthlySales = Enumerable.Range(0, 6)
                .Select(offset => currentMonth.AddMonths(offset - 5))
                .Select(month => new MonthlySalesVM
                {
                    Label = month.ToString("MMM"),
                    Amount = sales
                        .Where(s => s.SaleDate.Year == month.Year && s.SaleDate.Month == month.Month)
                        .Sum(s => s.TotalAmount)
                })
                .ToList();

            var topSellingProducts = sales
                .SelectMany(s => s.SaleItems)
                .Where(si => si.Product != null)
                .GroupBy(si => new { si.ProductId, si.Product.ProductName })
                .Select(g => new TopSellingProductVM
                {
                    ProductName = g.Key.ProductName,
                    UnitsSold = g.Sum(si => si.Quantity),
                    Revenue = g.Sum(si => si.TotalPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .ThenByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

            var activities = new List<DashboardActivityVM>();

            activities.AddRange(recentMovements.Select(sm => new DashboardActivityVM
            {
                Icon = sm.MovementType.Equals("OUT", StringComparison.OrdinalIgnoreCase) ? "⚠" : "↻",
                Title = sm.MovementType.Equals("OUT", StringComparison.OrdinalIgnoreCase)
                    ? "Stock moved out"
                    : "Stock updated",
                Detail = $"{sm.Product?.ProductName ?? "Unknown product"} ({sm.MovementType} {sm.Quantity})",
                Date = sm.MovementDate
            }));

            activities.AddRange(recentPurchases.Take(3).Select(p => new DashboardActivityVM
            {
                Icon = "🛒",
                Title = "New purchase recorded",
                Detail = $"Purchase #{p.PurchaseId} · {p.Supplier?.SupplierName ?? "Supplier"}",
                Date = p.PurchaseDate
            }));

            activities.AddRange(sales.Take(3).Select(s => new DashboardActivityVM
            {
                Icon = "💰",
                Title = "New sale recorded",
                Detail = $"Sale #{s.SaleId} · ₹{s.TotalAmount:N2}",
                Date = s.SaleDate
            }));

            activities.AddRange(outOfStock.Take(3).Select(p => new DashboardActivityVM
            {
                Icon = "!",
                Title = "Product out of stock",
                Detail = p.ProductName,
                Date = now
            }));

            var recentActivities = activities
                .OrderByDescending(a => a.Date)
                .Take(6)
                .ToList();

            return new DashboardVM
            {
                TotalProducts = totalProducts,
                TotalCategories = await _context.Categories.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalPurchases = await _context.Purchases.CountAsync(),
                TotalSales = sales.Count,
                LowStockCount = products.Count(p => p.Quantity <= p.ReorderLevel),
                InStockProducts = inStock.Count,
                LowStockProductsCount = lowStock.Count,
                OutOfStockProducts = outOfStock.Count,
                MonthlySales = monthlySales,
                TopSellingProducts = topSellingProducts,
                LowStockProducts = lowStock
                    .OrderBy(p => p.Quantity)
                    .Take(5)
                    .ToList(),
                RecentActivities = recentActivities,
                RecentPurchases = recentPurchases,
                RecentSales = sales.Take(5).ToList()
            };
        }
    }
}
