using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Sale
    {
        [Key]
        [Required(ErrorMessage = "Sale ID is required.")]
        public int SaleId { get; set; }

        [Required(ErrorMessage = "Sale date is required.")]
        public DateTime SaleDate { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, 9999999.99,
            ErrorMessage = "Total amount must be greater than 0.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Sale items are required.")]
        public ICollection<SaleItem> SaleItems { get; set; }
            = new List<SaleItem>();
    }
}