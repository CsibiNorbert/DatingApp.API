using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public class MessageParams
    {
        private const int MaxPageSize = 50;
        private int pageSize = 10;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }
        public int PageNumber { get; set; } = 1;

        // Criteria for filtering
        // Filtering out the currently logged in user and Gender
        public int UserId { get; set; }
        public string MessageContainer { get; set; } = "Unread"; // by default we pass back unread
    }
}
