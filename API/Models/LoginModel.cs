namespace API.Models
{
    public class LoginModel : IEquatable<LoginModel>
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }

        public LoginModel(string username, string password)
        {
            UserName = username;
            Password = password;
        }

        public bool Equals(LoginModel? other)
        {
            return UserName == other.UserName && Password == other.Password;
        }
    }
}
