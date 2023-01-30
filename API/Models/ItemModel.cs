namespace API.Models
{
    public class ItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }

        public ItemModel(int id, string name, int price, int quantity)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Price = price;
            Quantity = quantity;
        }
    }
}
