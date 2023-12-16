using Company_Software_Project_Documentation.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Company_Software_Project_Documentation.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>
            options)
            : base(options)
        { }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Project> Projects { get; set; }

    }
}
