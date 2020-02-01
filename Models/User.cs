using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace DatingApp.API.Models
{
    // Userid is using int Type, it will force to use the int as and id
    public class User : IdentityUser<int>
    {
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime CreatedProfile { get; set; }
        public DateTime LastActive { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        // Lazy loading data by using the virtual to state that this is a navigation property and it needs to be lazy loaded
        public virtual ICollection<Photo> Photos { get; set; }

        public virtual ICollection<Like> Likers { get; set; }
        public virtual ICollection<Like> Likees { get; set; }
        public virtual ICollection<Message> MessagesSent { get; set; }
        public virtual ICollection<Message> MessagesReceived { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}