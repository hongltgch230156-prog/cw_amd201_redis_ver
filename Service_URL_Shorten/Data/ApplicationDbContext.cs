using Microsoft.EntityFrameworkCore;
using Service_URL_Shorten.Models;
namespace Service_URL_Shorten.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Url> Urls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Url>()
                .HasIndex(u => u.ShortCode)
                .IsUnique();
        }
    
    }
}
