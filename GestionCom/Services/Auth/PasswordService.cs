using GestionCom.Entities;
using Microsoft.AspNetCore.Identity;

namespace GestionCom.Services.Auth
{
    public class PasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string HashPassword(User user, string password)
        {
            return _hasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string inputPassword)
        {
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, inputPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
