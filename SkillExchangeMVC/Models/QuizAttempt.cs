// QuizAttempt.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class QuizAttempt : BaseModel
    {
        [Key]
        public int QuizAttemptId { get; set; }
        [Required]
        public int QuizId { get; set; }
        [Required]
        public string UserInfoId { get; set; } = string.Empty; // student
        public DateTime AttemptedOn { get; set; } = DateTime.Now;
        public int Score { get; set; }
        public bool Passed { get; set; }
    }
}
