//PaymentViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.ViewModels
{
    public class PaymentViewModel
    {
        public Course? Course { get; set; } // To show course title and other info
        [Required]
        [Display(Name = "Card Number")]
        [CreditCard(ErrorMessage = "Invalid card number.")]
        public string? CardNumber { get; set; }

        [Required]
        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [Display(Name = "Card Holder Name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "CVV must be 3 or 4 digits")]
        public string? CVV { get; set; }
     }
 }

