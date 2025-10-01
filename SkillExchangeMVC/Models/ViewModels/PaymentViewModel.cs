//PaymentViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.ViewModels
{
    public class PaymentViewModel
    {
        // Course for display purposes only - no validation
        public Course? Course { get; set; }
        
        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Card"; // Card, Bkash

        // Card payment fields - conditional validation handled in controller
        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        public string? CVV { get; set; }

        // Bkash payment fields - conditional validation handled in controller
        [Display(Name = "Mobile Number")]
        public string? BkashMobileNumber { get; set; }

        [Display(Name = "Reference")]
        public string? PayerReference { get; set; }

        // Amount - basic validation
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
    }
}

