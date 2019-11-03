using DatingApp.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class Seed
    {
        // The method is static because we use the method without instanciating the class
        // We don`t use async because this method is being called when our application very first starts up ( called once )
        public static void SeedUsers(DataContext context)
        {
            // Check if DB is empty
            if (!context.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");

                // convert the Data to user objects
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                foreach (var user in users)
                {
                    byte[] passwordHash, passwordSalt;

                    CreatePasswordHash("password",out passwordHash,out passwordSalt);

                    user.HashPassword = passwordHash;
                    user.SaltPassword = passwordSalt;
                    user.Username = user.Username.ToLower();
                    context.Users.Add(user);
                }

                context.SaveChanges();
            }
        }
        // This is created here to seed the database with hashed password
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key; // This is used to decrypt the HASH generated for the password, it is called SALT
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); // This gives us the computed HASH for our password. The encoding statement means that it gives back the bytes from password
            }
        }
    }
}
