using FreeBooksAPI.Api.Models.Dtos;

namespace FreeBooksAPI.Api.Services;

public interface IAuthorService
{
    Task<PagedResult<AuthorDto>> GetAuthorsAsync(
        int page,
        int pageSize,
        string? sortBy = null,
        string? order = null);

    Task<PagedResult<BookDto>> GetBooksByAuthorAsync(
        string authorSlug,
        int page,
        int pageSize);
}