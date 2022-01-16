using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using TwittorApp.Models;

namespace TwittorApp.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<ApplicationDbContext>(), isProd);
            }
        }

        private static void SeedData(ApplicationDbContext context, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("--> Menjalankan Migrasi");
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Gagal melakukan migrasi {ex.Message}");
                }
            }

            if (!context.Roles.Any())
            {
                Console.WriteLine("--> Seeding data....");
                context.Roles.AddRange(
                    new Role() { Name = "Administrator" },
                    new Role() { Name = "Member" }
                );
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> Already have data...");
            }

            if (!context.Users.Any())
            {
                Console.WriteLine("--> Seeding data....");
                context.Users.AddRange(
                    new User() { FullName = "Oki Sabeni", 
                                 Email = "okisabeni@gmail.com", 
                                 Username = "okisab163", 
                                 Password = "$2a$11$hxGtcsyj7QxzKnME7yXh/e.qEdnvHmkEniPJn4MwFty91PnDhbnxi", 
                                 IsBanned = false, 
                                 UserCreated = DateTime.Now },
                    new User() { FullName = "Alviery Julian", 
                                 Email = "alviery@gmail.com", 
                                 Username = "alviery163", 
                                 Password = "$2a$11$hxGtcsyj7QxzKnME7yXh/e.qEdnvHmkEniPJn4MwFty91PnDhbnxi", 
                                 IsBanned = false, 
                                 UserCreated = DateTime.Now }
                );
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> Already have data...");
            }

            if (!context.UserRoles.Any())
            {
                Console.WriteLine("--> Seeding data....");
                context.UserRoles.AddRange(
                    new UserRole() { UserId = 1, RoleId = 1 },
                    new UserRole() { UserId = 2, RoleId = 2 }
                );
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> Already have data...");
            }

            if (!context.Twittors.Any())
            {
                Console.WriteLine("--> Seeding data....");
                context.Twittors.AddRange(
                    new Twittor() { TwittorContent = "Lorem Ipsum",  TwittorCreated = DateTime.Now, UserId = 1 },
                    new Twittor() { TwittorContent = "Ipsum Lorem", TwittorCreated = DateTime.Now, UserId = 2 }
                );
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> Already have data...");
            }

            if (!context.Comments.Any())
            {
                Console.WriteLine("--> Seeding data....");
                context.Comments.AddRange(
                    new Comment() { CommentContent = "Lorem Ipsum", CommentCreated = DateTime.Now, TwittorId = 1 },
                    new Comment() { CommentContent = "Ipsum Lorem", CommentCreated = DateTime.Now, TwittorId = 2 },
                    new Comment() { CommentContent = "Lorem Ipsum", CommentCreated = DateTime.Now, TwittorId = 2 },
                    new Comment() { CommentContent = "Ipsum Lorem", CommentCreated = DateTime.Now, TwittorId = 1 }
                );
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> Already have data...");
            }
        }
    }
}
