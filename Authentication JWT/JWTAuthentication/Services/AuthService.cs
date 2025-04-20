using JWTAuthentication.Data;
using JWTAuthentication.Entities;
using JWTAuthentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWTAuthentication.Services
{
    public class AuthService(UserDbContext dbContext, IConfiguration configuration, ILogger<User> logger) : IAuthService
    {
        public TokenDTO? LogIn(UserDTO user)
        {
            if(user == null)
            {
                logger.LogError("provide user details");
                return null;
            }

            User? obj = dbContext.Users.FirstOrDefault(userTemp => userTemp.UserName == user.UserName);
            
            if (obj == null)
            {
                logger.LogError($"{user.UserName} not registered");
                return null;
            }

            if(new PasswordHasher<User>().VerifyHashedPassword(obj, obj.PasswordHash, user.Password) == PasswordVerificationResult.Failed)
            {
                logger.LogError($"{user.Password} is not correct");
                return null;
            }

            return CreateAccessAndRefreshTokens(obj);
        }

        public bool Register(UserDTO user)
        {
            bool registered = dbContext.Users.Any(userTemp => userTemp.UserName == user.UserName);
            if (registered)
            {
                return false;
            }
            User userRes = new User();
            var passwordHasher = new PasswordHasher<User>().HashPassword(userRes, user.Password);
            userRes.UserName = user.UserName;
            userRes.PasswordHash = passwordHasher;
            userRes.Role = user.Role!;

            dbContext.Users.Add(userRes);
            dbContext.SaveChanges();
            return true;
        }

        public TokenDTO? GenerateNewToken(UserWithToken user)
        {
            User? obj = dbContext.Users.FirstOrDefault(temp => temp.UserName == user.UserName);
            if(obj is null || obj.RefrehToken != user.RefreshToken || obj.RefreshTokenExpireTime <= DateTime.Now)
            {
                return null;
            }

            return new TokenDTO
            {
                AccessToken = CreateAccessToken(obj),
                RefreshToken = CreateAndSaveRefreshToken(obj)
            };
        }

        private TokenDTO CreateAccessAndRefreshTokens(User user)
        {
            return new TokenDTO
            {
                AccessToken = CreateAccessToken(user),
                RefreshToken = CreateAndSaveRefreshToken(user)
            };
        }

        private string CreateAndSaveRefreshToken(User user)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            user.RefrehToken = Convert.ToBase64String(tokenBytes);
            user.RefreshTokenExpireTime = DateTime.Now.AddDays(1);
            dbContext.SaveChanges();
            return user.RefrehToken;
        }

        private string CreateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken
            (
                issuer: configuration.GetValue<string>("AppSettings:Issuer")!,
                audience: configuration.GetValue<string>("AppSettings:Audiences")!,
                claims: claims,
                expires : DateTime.UtcNow.AddHours(8).AddMinutes(32),
                signingCredentials: cred
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
