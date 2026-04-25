using GestionCom.Data;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services.Auth
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;

        public AuthService(AppDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);

            if (user == null)
                return null;

            var isValid = _passwordService.VerifyPassword(user, password);

            if (!isValid)
                return null;

            return user;
        }
    }
}
