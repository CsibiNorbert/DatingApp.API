namespace DatingApp.API.Models
{
    public class Like
    {
        // Likes a user
        // These are collections part of user entity
        public int LikerId { get; set; }

        // Who is liked by another user
        public int LikeeId { get; set; }

        public virtual User Liker { get; set; }
        public virtual User Likee { get; set; }
    }
}