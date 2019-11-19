namespace DatingApp.API.Helpers
{
    public class PaginationHeader
    {
        // This is the information that we pass in the HTTP response header
        // This is referenced in the Extensions class
        public int CurrentPage { get; set; }

        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalPages { get; set; }

        public PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            this.CurrentPage = currentPage;
            this.TotalItems = totalItems;
            this.ItemsPerPage = itemsPerPage;
            this.TotalPages = totalPages;
        }
    }
}