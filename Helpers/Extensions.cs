using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            // The key for the header will be pagination & passing the string values
            // We need to convert our object to a series of string of values using JsonConverter
            // Passing the value in camel case
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            response.Headers.Add("pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormatter));

            // we need to expose the headers so that we don`t take a CORS error
            response.Headers.Add("Access-Control-Expose-Headers", "pagination");
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
