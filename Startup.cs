using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.CloudinarySettings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Text;

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            // Getting access to our app configuration json
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This tun in Development mode
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            // inject the dbcontext with SQLite core. Download nuget for sqlite & add a section in appsetings.json with the connection string and specify here
            services.AddDbContext<DataContext>(c =>
            {
                c.UseLazyLoadingProxies();
                c.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });

            ConfigureServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            // inject the dbcontext with SQLite core. Download nuget for sqlite & add a section in appsetings.json with the connection string and specify here
            services.AddDbContext<DataContext>(c =>
            {
                c.UseLazyLoadingProxies();
                c.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            ConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // Inject services in other parts of the app. Dependency Injection
        public void ConfigureServices(IServiceCollection services)
        {
            // This is for web API controllers. v3.0+
            services.AddControllers() 
                    .AddNewtonsoftJson(opt => 
                    {
                        opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    });


            
            // Injecting the AuthRepo so that we can use it in the controllers
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IDatingRepository, DatingRepository>();

            // Add CORS service.
            services.AddCors();

            // the values inside the json gile will be assigned to the class when we get the values
            // They are going to match what is in the class
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

            // Scoped because we want to create new instance per request
            // And we can make use of this in the user controller with ServiceFilter
            services.AddScoped<LogUserActivity>();

            // we need to give an assembly to where to look in
            services.AddAutoMapper(typeof(DatingRepository).Assembly);
            // Add token scheme
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                    ValidateIssuer = false, // this is localhost
                    ValidateAudience = false // localhost
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // Add middleware to do something with the request
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Adds a middleware for the exception handler
                // Context relates to our http request/response
                // This doesn`t require a try/catch block
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                        // This will store our error
                        var error = context.Features.Get<IExceptionHandlerFeature>();

                        if (error != null)
                        {
                            // This will add a new header into our response
                            context.Response.AddApplicationError(error.Error.Message);

                            // writing our error message to the http response
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // Security header. Use HTTPS if available.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting(); // v3.0+ Adding routing middleware to our pipeline
            app.UseAuthentication();
            app.UseAuthorization(); // v3.0 This is in the template by default and it needs to be added
           

            // Configure the middlewear for CORS
            // Allow credentials will fix the uploader issue, but this means that we send cookies without request
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseDefaultFiles(); // it will look for something called index.html inside our content root path
            app.UseStaticFiles(); // support for using static files from the SPA in wwwroot folder.



            // We use a web api project and we need to use this instead of MVC below
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // this is for the publishing, for the fallback controller if there is any.
                // endpoints.MapFallbackToController("Index", "Fallback");
            });



            // MVC middlewear. It will tell to our API  if it doesn`t find the route for one of our controller end points,
            // The use the controller as a fallback and use that particullar action to serve our index page
            // Always fall back to this index and angular is taking care of our routing

            //app.UseMvc(routes =>
            //{
            //    routes.MapSpaFallbackRoute(
            //        name: "spa-fallback",
            //        defaults: new { controller = "Fallback", action = "Index" }
            //        );
            //});
        }
    }
}