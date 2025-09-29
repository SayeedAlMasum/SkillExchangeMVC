// QuizResponse.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class QuizResponse : BaseModel
    {
        [Key]
        public int QuizResponseId { get; set; }
        [Required]
        public int QuizAttemptId { get; set; }
        [Required]
        public int QuizQuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
    }
}
