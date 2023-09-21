using Microsoft.EntityFrameworkCore;
using NetCoreAuthAPI.Models;

namespace NetCoreAuthAPI.Context
{
    public class ApplicationDbContext : DbContext   
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");   
        }
    }
}
