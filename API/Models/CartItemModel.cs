namespace API.Models
{
    public class CartItemModel
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int SumPrice { get; set; }

        public CartItemModel(int id, int itemId, string name, int price, int quantity)
        {
            Id = id;
            ItemId = itemId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Price = price;
            Quantity = quantity;
            SumPrice = quantity * price;
        }
    }
}
