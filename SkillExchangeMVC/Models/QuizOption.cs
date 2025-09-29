// QuizOption.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class QuizOption : BaseModel
    {
        [Key]
        public int QuizOptionId { get; set; }
        [Required]
        public int QuizQuestionId { get; set; }
        [Required]
        [MaxLength(500)]
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Order { get; set; } = 0;
    }
}
