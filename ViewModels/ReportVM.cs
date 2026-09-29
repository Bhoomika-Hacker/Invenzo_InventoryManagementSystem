using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.ViewModels
{
    public class ReportVM
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public IEnumerable<Sale> Sales { get; set; }
            = new List<Sale>();

        public IEnumerable<Purchase> Purchases { get; set; }
            = new List<Purchase>();

        public IEnumerable<Product> Products { get; set; }
            = new List<Product>();

        public IEnumerable<StockMovement> StockMovements { get; set; }
            = new List<StockMovement>();

        public IEnumerable<Product> LowStockProducts { get; set; }
            = new List<Product>();
    }
}