// Enrollment.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class Enrollment : BaseModel
    {
        [Key]
        public int EnrollmentId { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string UserInfoId { get; set; } = string.Empty; // student
        public DateTime EnrolledOn { get; set; } = DateTime.Now;
    }
}
