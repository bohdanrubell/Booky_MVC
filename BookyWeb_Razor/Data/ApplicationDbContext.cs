using BookyWeb_Razor.Models;
using Microsoft.EntityFrameworkCore;

namespace BookyWeb_Razor.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option): base(option)
        {
            
        }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Razor_Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Razor_Roman", DisplayOrder = 2 },
                new Category { Id = 3, Name = "Razor_SciFi", DisplayOrder = 3 }
                );
        }

    }
}
