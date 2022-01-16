using Microsoft.EntityFrameworkCore;
using TwittorApp.Models;

namespace TwittorApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Twittor> Twittors { get; set; }
        public DbSet<Comment> Comments { get; set; }

    }
}
