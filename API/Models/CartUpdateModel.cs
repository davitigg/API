namespace API.Models
{
    public class CartUpdateModel
    {
        public int ItemId { get; set; }
        public int Step { get; set; }

        public CartUpdateModel(int itemId, int step)
        {
            ItemId = itemId;
            Step = step;
        }
    }
}
