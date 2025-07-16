namespace BlogService.Helpers;

public class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public static class PaginationHelper
{
    public static PaginatedResult<T> CreatePaginatedResult<T>(
        IEnumerable<T> data, 
        int page, 
        int pageSize, 
        int totalItems)
    {
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        
        return new PaginatedResult<T>
        {
            Data = data,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            }
        };
    }
}
