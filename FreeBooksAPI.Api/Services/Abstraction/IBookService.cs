using FreeBooksAPI.Api.Models.Dtos;


namespace FreeBooksAPI.Api.Services;

public interface IBookService
{
    Task<PagedResult<BookDto>> GetBooksAsync(
        int page,
        int pageSize,
        string? kind = null,
        string? genre = null,
        string? epoch = null,
        string? sortBy = null,
        string? order = null);

    Task<BookDto> GetBookBySlugAsync(string slug);
}
