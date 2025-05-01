namespace SkillExchangeMVC.Models.ViewModels
{
    public class CourseViewModel
    {
        public Course Course { get; set; } = new Course();
        public List<Course> Courses { get; set; }= new List<Course>();

    }
}
