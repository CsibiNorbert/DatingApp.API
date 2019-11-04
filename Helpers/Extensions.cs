using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    // We don't create a new instance when we want to use these methods
    public static class Extensions
    {
        // In this method we can add additional headers
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            // New header
            response.Headers.Add("application-error", message);

            //Allow the message to be displayed
            response.Headers.Add("Access-Control-Expose-Headers", "application-error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public static int CalculateAge(this DateTime theDateTime)
        {
            var age = DateTime.Today.Year - theDateTime.Year;
            if (theDateTime.AddYears(age) > DateTime.Today)
            {
                age--;
            }

            return age;
        }
    }
}
