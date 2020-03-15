using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : IdentityDbContext<User,Role,int, IdentityUserClaim<int>,UserRole, IdentityUserLogin<int>,IdentityRoleClaim<int>,IdentityUserToken<int>>
    {
        // we pass the DataContext class to the base class
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // We need to call this when we use identity.
            base.OnModelCreating(builder);
            // Many to many
            builder.Entity<UserRole>(userRole =>
            {
                //we give it a key that is a comb. of the userId and the roleId 
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                // Configure relationship side
                // Foreignkey cannot be assigned null, hence the isrequired
                userRole.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).IsRequired();
                userRole.HasOne(ur => ur.User).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.UserId).IsRequired();

            });
            // in between {} is the combination of the primary key.
            builder.Entity<Like>().HasKey(k => new { k.LikerId, k.LikeeId });
            // Fluent API to define the relationship
            // The delete is not cascading because we don`t want to delete the user when we delete a like
            builder.Entity<Like>()
                .HasOne(u => u.Likee)
                .WithMany(u => u.Likers)
                .HasForeignKey(u => u.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                .HasOne(u => u.Liker)
                .WithMany(u => u.Likees)
                .HasForeignKey(u => u.LikerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
               .HasOne(m => m.Recipient)
               .WithMany(m => m.MessagesReceived)
               .OnDelete(DeleteBehavior.Restrict);

            // *Global query filter that filters out everything(Photo) and send back photos that are approved
            // *If you browse like somebody else, they cannot see them, only the current user
            // By ignoring the query filters
            builder.Entity<Photo>().HasQueryFilter(p => p.IsApproved);
        }
    }
}