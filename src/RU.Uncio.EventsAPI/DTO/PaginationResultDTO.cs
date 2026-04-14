namespace RU.Uncio.EventsAPI.DTO
{
    public record PaginatedResultDTO<T>(IEnumerable<T> Items, int CurrentItems, int CurrentPage, int TotalPages, int TotalItems);
}
