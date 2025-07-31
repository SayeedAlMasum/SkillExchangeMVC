//UserInfo.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class UserInfo
    {
        [Key]
        [MaxLength(128)]
        public string UserInfoId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Location { get; set; } = "Unknown";

        public bool IsActive { get; set; } = true;

        [Required]
        public string Role { get; set; } = "Student"; // Add this property for role management
        public int RoleId { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
      
    }
}