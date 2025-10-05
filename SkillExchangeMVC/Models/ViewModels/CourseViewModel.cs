//CourseViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SkillExchangeMVC.Models.ViewModels
{
    public class CourseViewModel
    {
        public Course Course { get; set; } = new Course();
        public List<Course> Courses { get; set; }= new List<Course>();
        public List<SelectListItem>? Teachers { get; set; } = new List<SelectListItem>();
        // Track courses current logged-in user is already enrolled in to hide enroll button
        public List<int> EnrolledCourseIds { get; set; } = new List<int>();
    }
}
