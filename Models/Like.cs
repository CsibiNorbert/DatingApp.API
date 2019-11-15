using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Models
{
    public class Like
    {
        // Likes a user
        // These are collections part of user entity
        public int LikerId { get; set; }
        // Who is liked by another user
        public int LikeeId { get; set; }
        public User Liker { get; set; }
        public User Likee { get; set; }
    }
}
