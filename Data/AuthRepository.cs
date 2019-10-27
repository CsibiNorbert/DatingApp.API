using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=> u.Username == username);

            if (user == null)
            {
                return null;
            }

            if (!VerifyHashPassword(password, user.HashPassword,user.SaltPassword))
            {
                return null;
            }

            return user;
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

        private bool VerifyHashPassword(string password, byte[] hashPassword, byte[] saltPassword)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(saltPassword))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != hashPassword[i])
                    {
                        return false;
                    }
                }

            }
            return true;
        }
    }
}
