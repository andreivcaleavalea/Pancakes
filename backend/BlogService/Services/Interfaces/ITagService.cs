namespace BlogService.Services.Interfaces;

public interface ITagService
{
    /// <summary>
    /// Gets the most popular tags based on usage frequency
    /// </summary>
    /// <param name="limit">Maximum number of tags to return</param>
    /// <returns>List of popular tags ordered by usage frequency</returns>
    Task<IEnumerable<string>> GetPopularTagsAsync(int limit = 20);

    /// <summary>
    /// Searches for tags that match the given query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of tags to return</param>
    /// <returns>List of matching tags ordered by relevance</returns>
    Task<IEnumerable<string>> SearchTagsAsync(string query, int limit = 10);
}