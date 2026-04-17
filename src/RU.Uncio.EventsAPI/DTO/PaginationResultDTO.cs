namespace RU.Uncio.EventsAPI.DTO
{
    /// <summary>
    /// Pagination result
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    /// <param name="Items">Collection of items for the page</param>
    /// <param name="CurrentItems">Items number for the page</param>
    /// <param name="CurrentPage">Current page index</param>
    /// <param name="TotalPages">Total number of pages</param>
    /// <param name="TotalItems">Total number of items in the filtered collection</param>
    public record PaginatedResultDTO<T>(IEnumerable<T> Items, int CurrentItems, int CurrentPage, int TotalPages, int TotalItems);
}
