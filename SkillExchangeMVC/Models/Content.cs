//Content.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class Content
    {
        [Key]
        public int ContentId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string URL { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }
    }
}
