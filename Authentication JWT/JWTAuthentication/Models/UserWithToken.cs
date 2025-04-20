namespace JWTAuthentication.Models
{
    public class UserWithToken
    {
        public string UserName { get; set;}
        public string RefreshToken { get; set;}
    }
}
