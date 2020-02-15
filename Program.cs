using DatingApp.API.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // The host will Run after we have seeded our data
            var host = CreateHostBuilder(args).Build();

            // We create the scope because we cannot inject the context
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                // When we start the application we are going to seed the DB if users are not present
                // If we are messing with the migrations/Database, we can drop the DB and start the app again
                try
                {
                    var context = services.GetRequiredService<DataContext>();
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    // This will apply pending migrations and also will create the DB if it does not exist
                    context.Database.Migrate();

                    Seed.SeedUsers(userManager);
                }
                catch (System.Exception ex)
                {
                    // The ILogger needs to get the type of the class which is going to log against
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error occured during migration");
                }
            }

            // Now we can RUN it. In the begging we were running it in the main method, in the hostbuilder
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
                
    }
}