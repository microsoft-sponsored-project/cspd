using Company_Software_Project_Documentation.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Company_Software_Project_Documentation.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider
            serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                       serviceProvider.GetRequiredService
                           <DbContextOptions<ApplicationDbContext>>()))
            {

                if (context.Roles.Any())
                {
                    return; 
                }


                context.Roles.AddRange(
                    new IdentityRole
                    {
                        Id = "2c5e174e-3b0e-446f-86af483d56fd7210",
                        Name = "Admin",
                        NormalizedName = "Admin".ToUpper()
                    },
                    new IdentityRole
                    {
                        Id = "2c5e174e-3b0e-446f-86af483d56fd7211",
                        Name = "Editor",
                        NormalizedName = "Editor".ToUpper()
                    },
                    new IdentityRole
                    { Id = "2c5e174e-3b0e-446f-86af483d56fd7212", Name = "Guest", NormalizedName = "Guest".ToUpper() }
                );

                var hasher = new PasswordHasher<ApplicationUser>();

                context.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                        // primary key
                        UserName = "admin@csdoc.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@CSDOC.COM",
                        Email = "admin@csdoc.com",
                        NormalizedUserName = "ADMIN@CSDOC.COM",
                        PasswordHash = hasher.HashPassword(null,
                            "Admin1!")
                    },
                    new ApplicationUser
                    {

                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                        // primary key
                        UserName = "editor@csdoc.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "EDITOR@CSDOC.COM",
                        Email = "editor@csdoc.com",
                        NormalizedUserName = "EDITOR@CSDOC.COM",
                        PasswordHash = hasher.HashPassword(null,
                            "Editor1!")
                    },
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                        // primary key
                        UserName = "guest@csdoc.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "GUEST@CSDOC.COM",
                        Email = "guest@csdoc.com",
                        NormalizedUserName = "GUEST@CSDOC.COM",
                        PasswordHash = hasher.HashPassword(null,
                            "Guest1!")
                    }
                );

                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af483d56fd7210",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af483d56fd7211",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af483d56fd7212",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
