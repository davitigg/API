using API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenService
    {
        public string GenerateToken(UserModel user)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims = new List<Claim> {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.UserName!),
                new Claim("firstname", user.FName!),
                new Claim("lastname", user.LName!),
                new Claim("email", user.Email!),

            };

            var tokeOptions = new JwtSecurityToken(
                issuer: "https://localhost:5001",
                audience: "https://localhost:5001",
                expires: DateTime.Now.AddMinutes(60),
                claims: claims,
                signingCredentials: signinCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

            return tokenString;
        }
        public string GetData(JwtSecurityToken jwtToken, string key)
        {
            return jwtToken.Claims.First(claim => claim.Type == key).Value;
        }
    }
}
