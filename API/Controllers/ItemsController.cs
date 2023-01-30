using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly DBService dbService = new();
        private readonly TokenService tokenService = new();

        [HttpGet(), Authorize]
        public IActionResult GetItems()
        {
            try
            {
                List<ItemModel> list = dbService.SelectItems();
                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
        [HttpGet("cart"), Authorize]
        public IActionResult GetCartItems()
        {
            // get userId from jwt token
            var jwtToken = tokenService.GetToken(Request);
            var userId = int.Parse(tokenService.GetData(jwtToken, "id"));
            try
            {
                List<CartItemModel> list = dbService.SelectCartItems(userId);
                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
        [HttpPost("cart"), Authorize]
        public IActionResult UpdateCart([FromBody] CartUpdateModel cartUpdate)
        {
            // get userId from jwt token
            var jwtToken = tokenService.GetToken(Request);
            var userId = int.Parse(tokenService.GetData(jwtToken, "id"));
            var itemId = cartUpdate.ItemId;
            var step = cartUpdate.Step;

            CartItemModel cartItem = dbService.SelectCartItem(userId, itemId);
            ItemModel item = dbService.SelectItem(itemId);

            // check if item is remaining
            if (item.Quantity < 1 && step > 0)
            {
                return Conflict("ნივთის რაოდენობა ამოწურულია!");
            }

            if (cartItem != null)
            {
                if (cartItem.Quantity < 1 && step < 0)
                {
                    return BadRequest("invalid request!");
                }
                var newQuantity = step + cartItem.Quantity;

                // update cart item
                SqlCommand cmd = new("BEGIN TRANSACTION; " +
                    "UPDATE cart SET quantity=@cartQuantity WHERE userId=@userId AND itemId=@itemId " +
                    "UPDATE items SET quantity = @itemsQuantity WHERE items.id = @itemId; " +
                    "COMMIT;  ");

                // delete cart item if qnty=0
                if (newQuantity == 0)
                {
                    cmd = new("BEGIN TRANSACTION; " +
                        "DELETE FROM cart WHERE userId=@userId AND itemId=@itemId " +
                        "UPDATE items SET quantity = @itemsQuantity WHERE items.id = @itemId; " +
                        "COMMIT;  ");
                }
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@itemId", itemId);
                cmd.Parameters.AddWithValue("@cartQuantity", newQuantity);
                cmd.Parameters.AddWithValue("@itemsQuantity", item.Quantity - step);

                dbService.Update(cmd);

                return Ok("ნივთი კალათაში წარმატებით განახლდა!");
            }
            else
            {
                //insert new cart item
                SqlCommand cmd = new("BEGIN TRANSACTION; " +
                    "INSERT INTO cart (userId, itemId, quantity) VALUES (@userId, @itemId, @cartQuantity); " +
                    "UPDATE items SET quantity = @itemsQuantity WHERE items.id = @itemId; " +
                    "COMMIT;");
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@itemId", itemId);
                cmd.Parameters.AddWithValue("@cartQuantity", step);
                cmd.Parameters.AddWithValue("@itemsQuantity", item.Quantity - step);

                dbService.Insert(cmd);

                return Ok("ნივთი კალათაში წარმატებით დაემატა!");
            }
        }
        [HttpDelete("cart/delete/{id:int}"), Authorize]
        public IActionResult DeleteCartItem([FromRoute] int id)
        {
            // get userId from jwt token
            var jwtToken = tokenService.GetToken(Request);
            var userId = int.Parse(tokenService.GetData(jwtToken, "id"));
            var itemId = id;

            CartItemModel cartItem = dbService.SelectCartItem(userId, itemId);
            ItemModel item = dbService.SelectItem(itemId);

            SqlCommand cmd = new("BEGIN TRANSACTION; " +
                             "DELETE FROM cart WHERE userId=@userId AND itemId=@itemId " +
                             "UPDATE items SET quantity = @itemsQuantity WHERE items.id = @itemId; " +
                             "COMMIT;  ");

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.Parameters.AddWithValue("@itemsQuantity", item.Quantity + cartItem.Quantity);
            try
            {
                dbService.Delete(cmd);
                return Ok("ნივთი კალათიდან წაიშალა!");
            }
            catch (Exception)
            {
                return BadRequest("invalid request!");
            }

        }

    }
}
