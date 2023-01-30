using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Database
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) { }

        public DbSet<CartItem> Cart { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
