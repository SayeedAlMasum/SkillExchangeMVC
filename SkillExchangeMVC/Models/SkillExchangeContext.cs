using System;
using Microsoft.EntityFrameworkCore;

namespace SkillExchangeMVC.Models
{
    public class SkillExchangeContext : DbContext
    {
        public SkillExchangeContext(DbContextOptions<SkillExchangeContext> options) : base(options)
        {

        }

        public DbSet<Course> Course { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }
    }
}
