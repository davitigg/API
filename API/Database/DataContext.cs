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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User(
                    id: 1,
                    email: "test1@test",
                    password: "123456",
                    fName: "fnameTest1",
                    lName: "lnameTest1"),
                new User(
                    id: 2,
                    email: "test2@test",
                    password: "123456",
                    fName: "fnameTest2",
                    lName: "lnameTest2"));
            modelBuilder.Entity<Item>().HasData(
                new Item(
                    id: 1,
                    name: "iPhone 14 Pro",
                    price: 3000,
                    quantity: 10),
                 new Item(
                    id: 2,
                    name: "iPhone SE 2",
                    price: 2000,
                    quantity: 10),
                  new Item(
                    id: 3,
                    name: "Samsung Galaxy S22 Ultra",
                    price: 4000,
                    quantity: 10),
                   new Item(
                    id: 4,
                    name: "Samsung Fold 4",
                    price: 6000,
                    quantity: 10));
        }
    }
}
