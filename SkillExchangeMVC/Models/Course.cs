//Course.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillExchangeMVC.Models
{
    public class Course : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string SubCategory { get; set; } = string.Empty;

        [Required]
        public bool IsPremium { get; set; }  

        [Required]
        public string TeacherId { get; set; } = string.Empty;
    }
}
