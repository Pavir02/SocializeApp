namespace API.Helpers
{
    public class PaginationHeader
    {
        public PaginationHeader( int totalCount, int currentPage, int pageSize, int totalPages)
        {
            TotalCount  = totalCount;
            CurrentPage = currentPage;
            PageSize    = pageSize;
            TotalPages  = totalPages;
        }

        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

    }
}