using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class Billing
    {
        [Key]
        public int BillingId { get; set; }

        [Required]
        [StringLength(40)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public int SaleId { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        [StringLength(120)]
        public string CustomerName { get; set; } = "Walk-in Customer";

        [StringLength(160)]
        [EmailAddress]
        public string? CustomerEmail { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Outstanding";

        public Sale? Sale { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
