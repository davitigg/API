using API.Database;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly TokenService tokenService = new();
        private readonly DataContext context;
        public ItemsController(DataContext context)
        {
            this.context = context;
        }

        [HttpGet(), Authorize]
        public IActionResult Get()
        {
            try
            {
                var resp = this.GetAll();
                return Ok(resp);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
        [HttpPost("cart"), Authorize]
        public IActionResult UpdateCart([FromBody] CartUpdate cartUpdate)
        {
            // get userId from jwt token
            var jwtToken = tokenService.GetToken(Request);
            var userId = int.Parse(tokenService.GetData(jwtToken, "id"));
            var itemId = cartUpdate.ItemId;
            var step = cartUpdate.Step;

            CartItem cartItem = this.context.Cart.SingleOrDefault(i => i.User!.Id == userId && i.Item!.Id == itemId)!;
            Item item = this.context.Items.SingleOrDefault(i => i.Id == itemId)!;

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
                cartItem.Quantity += step;
                item.Quantity -= step;
                if (cartItem.Quantity == 0)
                {
                    this.context.Cart.Remove(cartItem);
                }
                else
                {
                    this.context.Cart.Update(cartItem);
                }
                this.context.Items.Update(item);
                this.context.SaveChanges();
                var resp = this.GetAll();
                return Ok(resp);
            }
            else
            {
                cartItem = new(0, userId, itemId, step);
                item.Quantity -= step;
                this.context.Cart.Add(cartItem);
                this.context.Items.Update(item);
                this.context.SaveChanges();
                var resp = this.GetAll();
                return Ok(resp);
            }
        }
        [HttpDelete("cart/delete/{id:int}"), Authorize]
        public IActionResult DeleteCartItem([FromRoute] int id)
        {
            // get userId from jwt token
            var jwtToken = tokenService.GetToken(Request);
            var userId = int.Parse(tokenService.GetData(jwtToken, "id"));
            var itemId = id;

            CartItem cartItem = this.context.Cart.SingleOrDefault(i => i.User!.Id == userId && i.Item!.Id == itemId)!;
            Item item = this.context.Items.SingleOrDefault(i => i.Id == itemId)!;
            item.Quantity += cartItem.Quantity;
            try
            {
                this.context.Cart.Remove(cartItem);
                this.context.Items.Update(item);
                this.context.SaveChanges();
                var resp = this.GetAll();
                return Ok(resp);
            }
            catch (Exception)
            {
                return BadRequest("invalid request!");
            }
        }
        private Dictionary<string, dynamic> GetAll()
        {
            // get userId from jwt token
            var jwtToken = tokenService.GetToken(Request);
            var userId = int.Parse(tokenService.GetData(jwtToken, "id"));

            List<Item> items = this.context.Items.ToList();
            List<CartItem> cart = this.context.Cart
                .Where(i => i.User!.Id == userId)
                .Include(i => i.User)
                .Include(i => i.Item)
                .ToList();
            cart.ForEach(i =>
            {
                i.User!.Password = "";
                i.SumPrice = i.Quantity * i.Item!.Price;
            });

            var resp = new Dictionary<string, dynamic>(){
                {"items", items },
                {"cart", cart }
            };
            return resp;
        }

    }
}
