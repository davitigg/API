namespace API.Database
{
    using API.Models;
    using System.Linq;

    public static class InitialData
    {
        public static void Seed(this DataContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
            dbContext.Database.EnsureCreated();
            if (!dbContext.Users.Any())
            {
                dbContext.Users.Add(new User
                (
                    id: 1,
                    email: "test1@test",
                    password: "123456",
                    fName: "fnameTest1",
                    lName: "lnameTest1"));

                dbContext.Users.Add(new User
                (
                    id: 2,
                    email: "test2@test",
                    password: "123456",
                    fName: "fnameTest2",
                    lName: "lnameTest2"));

                dbContext.SaveChanges();
            }

            if (!dbContext.Items.Any())
            {
                dbContext.Items.Add(new Item
                (
                    id: 1,
                    name: "iPhone 14 Pro",
                    price: 3000,
                    quantity: 10));

                dbContext.Items.Add(new Item
                (
                    id: 2,
                    name: "iPhone SE 2",
                    price: 2000,
                    quantity: 10));

                dbContext.Items.Add(new Item
                (
                    id: 3,
                    name: "Samsung Galaxy S22 Ultra",
                    price: 4000,
                    quantity: 10));

                dbContext.Items.Add(new Item
                (
                    id: 4,
                    name: "Samsung Fold 4",
                    price: 6000,
                    quantity: 10));

                dbContext.SaveChanges();
            }
        }
    }
}