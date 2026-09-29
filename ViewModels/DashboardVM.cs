using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.ViewModels
{
    public class DashboardVM
    {
        // Keep the original six dashboard cards.
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalPurchases { get; set; }
        public int TotalSales { get; set; }
        public int LowStockCount { get; set; }

        // Stock overview (product types, not individual units).
        public int InStockProducts { get; set; }
        public int LowStockProductsCount { get; set; }
        public int OutOfStockProducts { get; set; }

        // Six-month sales trend.
        public IEnumerable<MonthlySalesVM> MonthlySales { get; set; }
            = new List<MonthlySalesVM>();

        // Top selling products by quantity sold.
        public IEnumerable<TopSellingProductVM> TopSellingProducts { get; set; }
            = new List<TopSellingProductVM>();

        // Low-stock table.
        public IEnumerable<Product> LowStockProducts { get; set; }
            = new List<Product>();

        // Recent activity feed.
        public IEnumerable<DashboardActivityVM> RecentActivities { get; set; }
            = new List<DashboardActivityVM>();

        // Kept for compatibility with the existing dashboard/service code.
        public IEnumerable<Purchase> RecentPurchases { get; set; }
            = new List<Purchase>();

        public IEnumerable<Sale> RecentSales { get; set; }
            = new List<Sale>();
    }

    public class MonthlySalesVM
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class TopSellingProductVM
    {
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DashboardActivityVM
    {
        public string Icon { get; set; } = "•";
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
