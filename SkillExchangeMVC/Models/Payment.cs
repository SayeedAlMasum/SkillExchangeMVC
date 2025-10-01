//Payment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillExchangeMVC.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int CourseId { get; set; }

        [Required]
        public required string UserInfoId { get; set; }

        [Required]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        public string PaymentMethod { get; set; } = "Card"; // Card, Bkash

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        // Card payment fields
        public string? CardNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CVV { get; set; }

        // Bkash payment fields
        public string? BkashPaymentId { get; set; }
        public string? BkashTransactionId { get; set; }
        public string? BkashMerchantInvoiceNumber { get; set; }
        public string? BkashPayerReference { get; set; }

        // Navigation properties
        public Course? Course { get; set; }
        public UserInfo? UserInfo { get; set; }
    }
}