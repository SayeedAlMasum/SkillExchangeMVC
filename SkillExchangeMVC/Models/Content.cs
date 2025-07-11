//Content.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillExchangeMVC.Models
{
    public class Content : BaseModel
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
        public string UploaderEmail { get; set; } = string.Empty;
    }
}
