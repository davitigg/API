namespace API.Models
{
    public class CartUpdate
    {
        public int ItemId { get; set; }
        public int Step { get; set; }

        public CartUpdate(int itemId, int step)
        {
            ItemId = itemId;
            Step = step;
        }
    }
}
