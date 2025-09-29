//RequirementDocument.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models
{
    public class RequirementDocument
    {
        public int Id { get; set; }

        public string? FileName { get; set; }

        public string? UploadedBy { get; set; }

        public DateTime UploadDate { get; set; }
    }
}
