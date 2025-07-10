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
    }
}
