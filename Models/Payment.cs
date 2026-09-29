using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int BillingId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(30)]
        public string Method { get; set; } = "Pending";

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        [Required]
        [StringLength(80)]
        public string TransactionReference { get; set; } = string.Empty;

        [Required]
        public DateTime PaidAt { get; set; }

        public Billing? Billing { get; set; }
    }
}
