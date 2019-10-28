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
    }
}
