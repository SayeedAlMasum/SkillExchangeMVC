// QuizQuestion.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class QuizQuestion : BaseModel
    {
        [Key]
        public int QuizQuestionId { get; set; }
        [Required]
        public int QuizId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;
        public int Marks { get; set; } = 1;
        public int Order { get; set; } = 0;
    }
}
