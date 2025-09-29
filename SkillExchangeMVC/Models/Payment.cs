//Payment.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int CourseId { get; set; }

        [Required]
        public string UserInfoId { get; set; }

        [Required]
        public string PaymentStatus { get; set; } = "Pending";

        public string? CardNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? CVV { get; set; }
    }
}