using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DatingApp.API.Data
{
    public class Seed
    {
        // The method is static because we use the method without instanciating the class
        // We don`t use async because this method is being called when our application very first starts up ( called once )
        public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            // Check if we have any users in our DB
            if (!userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");

                // convert the Data to user objects
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                // create roles
                var roles = new List<Role>
                {
                    new Role{ Name="Member"},
                    new Role{ Name="Admin"},
                    new Role{ Name="Moderator"},
                    new Role{ Name="VIP"},
                };

                foreach (var role in roles)
                {
                    roleManager.CreateAsync(role).Wait();
                }

                foreach (var user in users)
                {
                    //byte[] passwordHash, passwordSalt;

                    //CreatePasswordHash("password", out passwordHash, out passwordSalt);

                    ////user.PasswordHash = passwordHash;
                    ////user.SaltPassword = passwordSalt;
                    //user.UserName = user.UserName.ToLower();
                    //context.Users.Add(user);

                    // Password is being passed for everyone the same, for simplicty
                    // We use wait to wait for the create method because we are not in an async method
                    userManager.CreateAsync(user, "password").Wait();
                    userManager.AddToRoleAsync(user, "Member").Wait();
                    // By default the isApproved is false
                    user.Photos.SingleOrDefault().IsApproved = true;
                }

                // Create admin user

                var adminUser = new User
                {
                    UserName = "Admin"
                };

                var result = userManager.CreateAsync(adminUser, "password").Result;

                if (result.Succeeded)
                {
                    var admin = userManager.FindByNameAsync("Admin").Result;
                    userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
                }
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