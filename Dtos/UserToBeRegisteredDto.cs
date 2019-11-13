using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Dtos
{
    public class UserToBeRegisteredDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(10,MinimumLength = 6,ErrorMessage = "Your password must be in between 6 and 10 characters")]
        public string Password { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string KnownAs { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        public DateTime CreatedProfile { get; set; }
        public DateTime LastActive { get; set; }

        // we don`t provide this to our form, so we construct the values
        public UserToBeRegisteredDto()
        {
            CreatedProfile = DateTime.Now;
            LastActive = DateTime.Now;
        }
    }
}
