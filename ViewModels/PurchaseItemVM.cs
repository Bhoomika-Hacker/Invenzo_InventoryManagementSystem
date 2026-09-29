using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.ViewModels
{
    public class PurchaseItemVM
    {
        [Required(ErrorMessage = "Purchase Item ID is required.")]
        public int PurchaseItemId { get; set; }

        [Required(ErrorMessage = "Purchase ID is required.")]
        public int PurchaseId { get; set; }

        [Required(ErrorMessage = "Please select a product.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, 100000,
            ErrorMessage = "Quantity must be greater than or equal to 0.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0.01, 9999999.99,
            ErrorMessage = "Unit price must be greater than 0.")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Total price is required.")]
        [Range(0.01, 99999999.99,
            ErrorMessage = "Total price must be greater than 0.")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Purchase is required.")]
        public Purchase Purchase { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public Product Product { get; set; }
    }
}