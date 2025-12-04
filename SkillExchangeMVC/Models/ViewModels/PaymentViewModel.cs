//PaymentViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.ViewModels
{
    public class PaymentViewModel
    {
        // Course for display purposes only - no validation
        public Course? Course { get; set; }

        // Card payment fields
        [Required(ErrorMessage = "Card number is required.")]
        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry date is required.")]
        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVV is required.")]
        [Display(Name = "CVV")]
        public string? CVV { get; set; }

        // Amount - basic validation
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
    }
}

