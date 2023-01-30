using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("Cart")]
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("User")]
        private int UserId { get; set; }
        public User? User { get; set; }
        [ForeignKey("Item")]
        private int ItemId { get; set; }
        public Item? Item { get; set; }
        public int Quantity { get; set; }
        [NotMapped]
        public int SumPrice { get; set; }

        public CartItem(int id, int userId, int itemId, int quantity)
        {
            Id = id;
            UserId = userId;
            ItemId = itemId;
            Quantity = quantity;
        }
    }
}
