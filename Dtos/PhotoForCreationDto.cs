using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Dtos
{
    public class PhotoForCreationDto
    {
        public string Url { get; set; }

        // This is the photo uploaded
        public IFormFile File { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        // This is coming back rom cloudinary
        public string PublicPhotoId { get; set; }

        // The reason is for adding the date time here
        public PhotoForCreationDto()
        {
            DateAdded = DateTime.Now;
        }
    }
}
