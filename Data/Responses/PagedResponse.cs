
namespace Data.Responses
{
    public class PagedResponse<T>
    {
        public T Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public PagedResponse(T data, int pageNumber, int pageSize, int totalRecords, bool success, string message)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            // Calculate total pages
            TotalPages = totalRecords > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 0;
            Success = success;
            Message = message;
        }
    }
}
