﻿namespace JWTAuthentication.Models
{
    public class UserDTO
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? Role { get; set; }
    }
}
