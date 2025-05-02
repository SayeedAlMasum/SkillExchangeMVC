//RegisterViewModel
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required , MaxLength(50)]
        public string Name { get; set; } = string.Empty;  
        
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }= string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
