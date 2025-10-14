using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NZWalks.API.Models.Domain;
using NZWalks.API.Repositories;

namespace NZWalks.API.Data
{
    public class NZWalksAuthDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public NZWalksAuthDbContext(DbContextOptions<NZWalksAuthDbContext> options) : base(options)
        {
        }

        /// This is the table that will be created in the database
        /// // for the refresh tokens. The RefreshToken class is in the Models/Domain folder
        public DbSet<RefreshToken> RefreshTokens { get; set; } // This is the table that will be created in the database
        public DbSet<Member> Members { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Roles
            var readerRoleId = "bf63e81d-8637-4392-a6d5-40d78d9f7566";
            var writerRoleId = "3707e4aa-2e65-41b3-a6c3-a2e212832522";
            var adminRoleId = "b3288a15-3794-4522-a9f8-1422b5e56999";

            var roles = new List<IdentityRole>
                {
                    new IdentityRole
                    {
                        Id = readerRoleId,
                        ConcurrencyStamp = readerRoleId,
                        Name = "Reader",
                        NormalizedName = "Reader".ToUpper(),
                    },
                    new IdentityRole
                    {
                        Id= writerRoleId,
                        ConcurrencyStamp = writerRoleId,
                        Name = "Writer",
                        NormalizedName = "Writer".ToUpper()
                    },
                    new IdentityRole
                    {
                        Id = adminRoleId,
                        ConcurrencyStamp = adminRoleId,
                        Name = "Admin",
                        NormalizedName = "Admin".ToUpper() // Normalized name is usually uppercase
                    }
                };

            builder.Entity<IdentityRole>().HasData(roles);  //if the database does not have these roles
                                                            //this will seed these data into the database
                                                            //to provide a list of roles to the entity

            //// Create a new user SuperAdmin for testing
            //var superAdminId = "f7b3e81d-8637-4392-a6d5-40d78d9f7566";
            //var superAdminUser = new IdentityUser
            //{
            //    UserName = "superadmin@NZWalks.com",
            //    Email = "superadmin@NZWalks.com",
            //    NormalizedEmail = "superadmin@NZWalks.com".ToUpper(),
            //    NormalizedUserName = "superadmin@NZWalks.com".ToUpper(),
            //    Id = superAdminId,
            //    ConcurrencyStamp = superAdminId
            //};
            //// Hash the password
            //superAdminUser.PasswordHash = new PasswordHasher<IdentityUser>()
            //    .HashPassword(superAdminUser, "Superadmin@123");

            //builder.Entity<IdentityUser>().HasData(superAdminUser);

            ////Add all Roles to SuperAdminUser Role
            //var superAdminRoles = new List<IdentityUserRole<string>> // this is a list of roles that the super admin user will have 
            //                                                         // IdentityUserRole needs a key and the key is a string
            //    {
            //        new IdentityUserRole<string>
            //        {
            //            RoleId = readerRoleId,
            //            UserId = superAdminId
            //        },
            //        new IdentityUserRole<string>
            //        {
            //            RoleId = writerRoleId,
            //            UserId = superAdminId
            //        }

            //    };
            //builder.Entity<IdentityUserRole<string>>().HasData(superAdminRoles);
        }
    }
}
