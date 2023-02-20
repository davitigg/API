using API.Database;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenService tokenService = new();
        private readonly DataContext _context;

        public AuthController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Auth/Users
        [HttpGet("Users"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users
                .ToListAsync();

            if (users.Count == 0)
            {
                return NotFound();
            }
            return users;

        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login loginModel)
        {
            if (loginModel is null)
            {
                return BadRequest("Invalid client request");
            }
            if (loginModel.Email!.Contains(' ') || loginModel.Password!.Contains(' '))
            {
                return Unauthorized();
            }
            try
            {
                var user = _context.Users
                                .Single(user => user.Email == loginModel.Email && user.Password == loginModel.Password);
                var tokenString = tokenService.GenerateToken(user);
                return Ok(new AuthenticatedResponse { Token = tokenString });
            }
            catch (Exception)
            {
                return Unauthorized("შეამოწმეთ მომხმარებელი და პაროლი");
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user is null)
            {
                return BadRequest("Invalid client request");
            }
            if (!user.CheckValidity())
            {
                return BadRequest("გთხოვთ შეიყვანოთ ვალიდური მონაცემები");
            }
            try
            {
                var userExist = _context.Users.SingleOrDefault(u => u.Email == user.Email);
                if (userExist == null)
                {
                    _context.Users.Add(user);
                    _context.SaveChanges();
                    return Login(new Login(user.Email, user.Password));
                }
                else return Conflict("ასეთი მომხმარებელი უკვე არსებობს");
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpDelete("delete")]
        public IActionResult Delete()
        {
            Request.Headers.TryGetValue("authorization", out var token);
            string jwtTokenString = token.ToString().Replace("Bearer ", "");
            var jwtToken = new JwtSecurityToken(jwtTokenString);
            var id = int.Parse(tokenService.GetData(jwtToken, "id"));
            try
            {
                var User = _context.Users.Single(u => u.Id == id);
                _context.Users.Remove(User);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPut("UpdateUser"), Authorize]
        public IActionResult UpdateUser([FromBody] User updatedUser)
        {
            if (updatedUser is null)
            {
                return BadRequest("Invalid client request");
            }
            // get userId from jwt token
            var jwtToken = tokenService.GetToken(Request);
            var userId = int.Parse(tokenService.GetData(jwtToken, "id"));
            try
            {
                // check if new username exists in the database
                if (_context.Users.SingleOrDefault(u => u.Email == updatedUser.Email && u.Id != userId) != null)
                {
                    return Conflict("ასეთი მომხმარებელი უკვე არსებობს");
                }
                // get user from the database
                var user = _context.Users.SingleOrDefault(u => u.Id == userId);
                // apply user changes
                user!.Email = updatedUser.Email;
                if (!(updatedUser.Password.Trim().Length == 0))
                {
                    user.Password = updatedUser.Password;
                }
                user.FName = updatedUser.FName;
                user.LName = updatedUser.LName;
                user.Email = updatedUser.Email;

                if (user.CheckValidity())
                {
                    // update user in the database
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    return Login(new Login(user.Email, user.Password));
                }
                return BadRequest("გთხოვთ შეიყვანეთ ვალიდური მონაცემები");
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}

