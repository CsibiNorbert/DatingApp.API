using System;

namespace DatingApp.API.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }

        // This is returned from cloudinary when we store a photo
        public string PublicPhotoId { get; set; }

        public virtual User User { get; set; }
        public int UserId { get; set; }
    }
}