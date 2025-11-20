using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using CrudCoursework.Models;

namespace CrudCoursework.Data
{
    public class UrlDbContext : DbContext
    {
        public UrlDbContext(DbContextOptions<UrlDbContext> options) : base(options) { }

        public DbSet<UrlMapping> Urls { get; set; }
    }
}
