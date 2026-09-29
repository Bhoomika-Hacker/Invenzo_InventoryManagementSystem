
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, 100000, ErrorMessage = "Quantity must be between 0 and 100000.")]
        public int Quantity { get; set; }

        // Purchase Price
        [Required(ErrorMessage = "Purchase price is required.")]
        [Range(0.01, 9999999.99,
            ErrorMessage = "Purchase price must be greater than 0.")]
        public decimal PurchasePrice { get; set; }

        // Selling Price
        [Required(ErrorMessage = "Selling price is required.")]
        [Range(0.01, 9999999.99,
            ErrorMessage = "Selling price must be greater than 0.")]
        public decimal SellingPrice { get; set; }

        [Required(ErrorMessage = "Reorder level is required.")]
        [Range(0, 100000, ErrorMessage = "Reorder level must be between 0 and 100000.")]
        public int ReorderLevel { get; set; }

        // Foreign Keys
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        // Navigation Properties
        public Category Category { get; set; }

        public Supplier Supplier { get; set; }
    }
}
