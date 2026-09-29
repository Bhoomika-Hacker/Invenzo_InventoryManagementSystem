using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Purchase
    {
        [Key]
        [Required(ErrorMessage = "Purchase ID is required.")]
        public int PurchaseId { get; set; }

        [Required(ErrorMessage = "Please select a supplier.")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Purchase date is required.")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, 9999999.99,
            ErrorMessage = "Total amount must be greater than 0.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Supplier is required.")]
        public Supplier Supplier { get; set; }

        [Required(ErrorMessage = "Purchase items are required.")]
        public ICollection<PurchaseItem> PurchaseItems { get; set; }
            = new List<PurchaseItem>();
    }
}