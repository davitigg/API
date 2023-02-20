using API.Database;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly TokenService tokenService = new();

        public UsersController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> PutUser(int id, User updatedUser)
        {
            // get userId from jwt token
            var jwtToken = tokenService.GetToken(Request);
            var userId = Int32.Parse(tokenService.GetData(jwtToken, "id"));
            if (userId != id && _context.Users.FindAsync(userId).Result!.Role != UserRole.Admin)
            {
                return Unauthorized();
            }
            if (id != updatedUser.Id)
            {
                return BadRequest();
            }

            // check if new username exists in the database
            if (await _context.Users.SingleOrDefaultAsync(u => u.Email == updatedUser.Email && u.Id != id) != null)
            {
                return Conflict("User with this email is already registered");
            }
            // get user from the database
            var user = _context.Users.FindAsync(id).Result;
            // apply user changes
            user!.Email = updatedUser.Email;
            if (!(updatedUser.Password.Trim().Length == 0))
            {
                user.Password = updatedUser.Password;
            }
            user.FName = updatedUser.FName;
            user.LName = updatedUser.LName;
            user.Role = updatedUser.Role;

            if (!user.CheckValidity())
            {
                return BadRequest("Invalid credentials");
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // get userId from jwt token
            var jwtToken = tokenService.GetToken(Request);
            var userId = Int32.Parse(tokenService.GetData(jwtToken, "id"));
            if (userId != id && _context.Users.FindAsync(userId).Result!.Role != UserRole.Admin)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Users
        [HttpDelete(), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUsers(int[] ids)
        {
            var users = _context.Users
               .Where(p => ids.Contains(p.Id));

            if (users.IsNullOrEmpty())
            {
                return NotFound();
            }

            foreach (var u in users)
            {
                _context.Users.Remove(u);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
