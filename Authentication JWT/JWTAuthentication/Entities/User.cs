using System.ComponentModel.DataAnnotations;

namespace JWTAuthentication.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Role {  get; set; }
        public string? RefrehToken { get; set; }
        public DateTime? RefreshTokenExpireTime { get; set; }
    }
}
