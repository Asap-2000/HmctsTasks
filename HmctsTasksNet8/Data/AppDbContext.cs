using HmctsTasks.Models;
using Microsoft.EntityFrameworkCore;

namespace HmctsTasks.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var task = modelBuilder.Entity<TaskItem>();

            task.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            task.Property(t => t.Description)
                .HasMaxLength(1000);

            task.Property(t => t.Status)
                .IsRequired();

            task.Property(t => t.DueAt)
                .IsRequired();

            task.Property(t => t.CreatedAt)
                .IsRequired();
        }
    }
}
