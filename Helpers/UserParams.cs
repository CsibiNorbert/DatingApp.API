namespace DatingApp.API.Helpers
{
    public class UserParams
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

        public string Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int Maxage { get; set; } = 99;

        // This is for last active
        public string OrderBy { get; set; }

        public bool Likees { get; set; } = false;
        public bool Likers { get; set; } = false;
    }
}