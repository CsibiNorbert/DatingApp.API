using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }
        public Task<User> Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<User> Register(User user, string password)
        {
            // These variables are referenced in the CreateHash method with out keyword.
            byte[] passwordHash, passwordSalt;

            CreateHashSaltPassword(password, out passwordHash,out passwordSalt);

            user.HashPassword = passwordHash;
            user.SaltPassword = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public Task<bool> UserExists(string username)
        {
            throw new NotImplementedException();
        }

        // We don't return anything here, we are setting the passwordHash & passwordSalt by using the out keyword.
        private void CreateHashSaltPassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key; // This is used to decrypt the HASH generated for the password, it is called SALT
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); // This gives us the computed HASH for our password. The encoding statement means that it gives back the bytes from password
            }
        }
    }
}
