//SkillExchangeContext
using System;
using Microsoft.EntityFrameworkCore;

namespace SkillExchangeMVC.Models.Context
{
    public class SkillExchangeContext : DbContext
    {
        public SkillExchangeContext(DbContextOptions<SkillExchangeContext> options) : base(options)
        {

        }

        public DbSet<Course> Course { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Content> Content { get; set; }
        public DbSet<RequirementDocument> RequirementDocuments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<QuizResponse> QuizResponses { get; set; }
        public DbSet<Payment> Payment { get; set; }
    }
}

