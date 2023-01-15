using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DBService dbService = new();
        private readonly TokenService tokenService = new();

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            if (loginModel is null)
            {
                return BadRequest("Invalid client request");
            }
            if (loginModel.UserName!.Contains(' ') || loginModel.Password!.Contains(' '))
            {
                return Unauthorized();

            }

            // selects user
            SqlCommand cmd = new("SELECT * FROM users WHERE username=@username AND password=@password");
            cmd.Parameters.AddWithValue("@username", loginModel.UserName);
            cmd.Parameters.AddWithValue("@password", loginModel.Password);

            try
            {
                var user = dbService.SelectUser(cmd);
                var tokenString = tokenService.GenerateToken(user);
                return Ok(new AuthenticatedResponse { Token = tokenString });
            }
            catch (Exception)
            {
                return Unauthorized();
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserModel user)
        {
            if (user is null)
            {
                return BadRequest("Invalid client request");
            }
            if (user.CheckValidity())

            {
                // checks if userame exists in database
                SqlCommand cmd = new("SELECT 1 FROM users WHERE username=@username");
                cmd.Parameters.AddWithValue("@username", user.UserName);
                var userExist = dbService.IsRowSelected(cmd);

                if (!userExist)
                {
                    // insert new user into the database
                    cmd = new("INSERT INTO users (username, password, first_name, last_name, email) Output Inserted.id " +
                       "VALUES (@username, @password, @first_name, @last_name, @email)");
                    cmd.Parameters.AddWithValue("@username", user.UserName);
                    cmd.Parameters.AddWithValue("@password", user.Password);
                    cmd.Parameters.AddWithValue("@first_name", user.FName);
                    cmd.Parameters.AddWithValue("@last_name", user.LName);
                    cmd.Parameters.AddWithValue("@email", user.Email);

                    try
                    {
                        dbService.Insert(cmd);
                        return Login(new LoginModel(user.UserName, user.Password));

                    }
                    catch (Exception)
                    {

                        return StatusCode(500);
                    }
                }
                else return Conflict("ასეთი მომხმარებელი უკვე არსებობს");
            }
            else return BadRequest("გთხოვთ შეიყვანოთ ვალიდური მონაცემები");
        }

        [HttpDelete("delete"), Authorize]
        public IActionResult Delete()
        {
            Request.Headers.TryGetValue("authorization", out var token);
            string jwtTokenString = token.ToString().Replace("Bearer ", "");
            var jwtToken = new JwtSecurityToken(jwtTokenString);

            var id = int.Parse(tokenService.GetData(jwtToken, "id"));

            SqlCommand cmd = new("DELETE FROM users WHERE users.id=@id");
            cmd.Parameters.AddWithValue("@id", id);
            try
            {
                int rowsAffected = dbService.Delete(cmd);
                return Ok();

            }
            catch (Exception)
            {
                return StatusCode(500);
            }

        }

        [HttpPut("UpdateUser"), Authorize]
        public IActionResult UpdateUser([FromBody] UserModel user)
        {

            if (user is null)
            {
                return BadRequest("Invalid client request");
            }

            // get userId from jwt token
            Request.Headers.TryGetValue("authorization", out var token);
            string jwtTokenString = token.ToString().Replace("Bearer ", "");
            var jwtToken = new JwtSecurityToken(jwtTokenString);
            var userId = int.Parse(tokenService.GetData(jwtToken, "id"));

            // get user from the database
            SqlCommand cmd = new("SELECT * FROM users WHERE id=@id");
            cmd.Parameters.AddWithValue("@id", userId);
            try
            {
                var userToBeUpdated = dbService.SelectUser(cmd);

                // check if new username exists in the database
                cmd = new("SELECT * FROM users WHERE username=@username AND NOT id=@id");
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@username", user.UserName);
                var usernameExists = dbService.IsRowSelected(cmd);
                if (usernameExists)
                {
                    return Conflict("ასეთი მომხმარებელი უკვე არსებობს");
                }

                // apply user changes
                userToBeUpdated.UserName = user.UserName;
                if (!(user.Password.Trim().Length == 0))
                {
                    userToBeUpdated.Password = user.Password;
                }
                userToBeUpdated.FName = user.FName;
                userToBeUpdated.LName = user.LName;
                userToBeUpdated.Email = user.Email;


                if (userToBeUpdated.CheckValidity())
                {
                    // update user in the database
                    cmd = new("UPDATE users " +
                       "SET username = @username, password = @password, first_name = @first_name, last_name = @last_name, email=@email " +
                       "WHERE users.id=@id;");
                    cmd.Parameters.AddWithValue("@id", userToBeUpdated.Id);
                    cmd.Parameters.AddWithValue("@username", userToBeUpdated.UserName);
                    cmd.Parameters.AddWithValue("@password", userToBeUpdated.Password);
                    cmd.Parameters.AddWithValue("@first_name", userToBeUpdated.FName);
                    cmd.Parameters.AddWithValue("@last_name", userToBeUpdated.LName);
                    cmd.Parameters.AddWithValue("@email", userToBeUpdated.Email);

                    try
                    {
                        dbService.Insert(cmd);
                        return Login(new LoginModel(userToBeUpdated.UserName, userToBeUpdated.Password));

                    }
                    catch (Exception)
                    {

                        return Unauthorized();
                    }
                }
                else return BadRequest("გთხოვთ შეიყვანეთ ვალიდური მონაცემები");
            }
            catch (Exception)
            {
                return StatusCode(500);

            }
        }
    }
}

