// Certificate.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class Certificate : BaseModel
    {
        [Key]
        public int CertificateId { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string UserInfoId { get; set; } = string.Empty; // student
        [Required]
        public string CertificateNumber { get; set; } = Guid.NewGuid().ToString("N").ToUpper();
        public DateTime IssuedOn { get; set; } = DateTime.Now;
        public int Score { get; set; }
        public string Grade { get; set; } = string.Empty;
    }
}
