using JWTAuthentication.Models;

namespace JWTAuthentication.Services
{
    public interface IAuthService
    {
        bool Register(UserDTO user);
        TokenDTO? LogIn(UserDTO user);
        TokenDTO? GenerateNewToken(UserWithToken user);
    }
}
