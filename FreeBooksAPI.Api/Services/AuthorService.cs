using FreeBooksAPI.Api.Models.Dtos;

namespace FreeBooksAPI.Api.Services;

public class AuthorService : IAuthorService
{
    private readonly IFreeBooksClient _freeBooksClient;
    private readonly ILogger<AuthorService> _logger;
    public AuthorService(IFreeBooksClient freeBooksClient, ILogger<AuthorService> logger)
    {
        freeBooksClient = _freeBooksClient;
        _logger = logger;
    }
    public Task<PagedResult<AuthorDto>> GetAuthorsAsync(int page, int pageSize, string? sortBy = null, string? order = null)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<BookDto>> GetBooksByAuthorAsync(string authorSlug, int page, int pageSize)
    {
        throw new NotImplementedException();
    }
}