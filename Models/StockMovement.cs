using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class StockMovement
    {
        [Key]
        public int StockMovementId { get; set; }

        [Required(ErrorMessage = "Please select a product.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Movement type is required.")]
        [StringLength(20)]
        public string MovementType { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 100000, ErrorMessage = "Quantity must be between 1 and 100000.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Movement date is required.")]
        public DateTime MovementDate { get; set; }

        [StringLength(200, ErrorMessage = "Reference cannot exceed 200 characters.")]
        public string Reference { get; set; }

        // Navigation Property
        public Product Product { get; set; }
    }
}