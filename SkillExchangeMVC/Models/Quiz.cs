// Quiz.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class Quiz : BaseModel
    {
        [Key]
        public int QuizId { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        public int TotalMarks { get; set; } = 100;
        public bool IsExam { get; set; } // true: exam, false: quiz
    }
}
