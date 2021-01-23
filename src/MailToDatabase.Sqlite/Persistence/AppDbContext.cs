using MailToDatabase.Sqlite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MailToDatabase.Sqlite.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public DbSet<Email> Emails { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
    }
}
