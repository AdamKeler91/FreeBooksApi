using FreeBooksAPI.Api.Models.Dtos;
using FreeBooksAPI.Api.Models.External;

namespace FreeBooksAPI.Api.Services;

public class AuthorService : IAuthorService
{
    private readonly IFreeBooksClient _freeBooksClient;
    private readonly ILogger<AuthorService> _logger;

    public AuthorService(
        IFreeBooksClient freeBooksClient,
        ILogger<AuthorService> logger)
    {
        _freeBooksClient = freeBooksClient;
        _logger = logger;
    }

    public async Task<PagedResult<AuthorDto>> GetAuthorsAsync(int page, int pageSize, string? sortBy = null, string? order = null)
    {
        _logger.LogInformation(
             "Fetching authors: page={Page}, pageSize={PageSize}, sortBy={SortBy}, order={Order}",
             page, pageSize, sortBy, order);

        // 1.Fetching Data

        var authorsList = await _freeBooksClient.GetListOfAuthorsAsync();

        // 2. Map to DTO

        var mappedAuthors = authorsList.Select(a => new AuthorDto(
            Slug: a.Slug,
            Name: a.Name
        )).ToList();

        IEnumerable<AuthorDto> sortedAuthors = mappedAuthors;

        // 3. Sort

        var sortByLower = sortBy?.ToLower() ?? "name";
        var orderLower = order?.ToLower() ?? "asc";

        if (sortByLower == "name")
        {
            if (orderLower == "desc")
                sortedAuthors = sortedAuthors.OrderByDescending(a => a.Name);
            else
                sortedAuthors = sortedAuthors.OrderBy(a => a.Name);
        }
        else
        {
            // Default: sort by name ascending
            sortedAuthors = sortedAuthors.OrderBy(a => a.Name);
        }

        // 4. Paginate
        var totalCount = sortedAuthors.Count();
        var items = sortedAuthors.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        _logger.LogInformation(
            "Returning {ItemCount} authors out of {TotalCount}",
            items.Count, totalCount);

        return new PagedResult<AuthorDto>
        (
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling((double)totalCount / pageSize)

        );
    }

    public async Task<PagedResult<BookDto>> GetBooksByAuthorAsync(string authorSlug, int page, int pageSize)
    {
        _logger.LogInformation(
            "Fetching books by author: authorSlug={AuthorSlug}, page={Page}, pageSize={PageSize}",
            authorSlug, page, pageSize);

        // 1. Fetch books by author
        var books = await _freeBooksClient.GetBooksByAuthorAsync(authorSlug);

        // 2. Fetch all authors (for mapping author names to slugs in multi-author books)
        var allAuthors = await _freeBooksClient.GetListOfAuthorsAsync();
        var authorsByName = allAuthors.ToDictionary(
            a => a.Name,
            StringComparer.OrdinalIgnoreCase);

        // 3. Map to DTOs
        var mappedBooks = books
            .Select(b => MapToBookDto(b, authorsByName))
            .ToList();

        // 4. Paginate (no sorting for books by author - keep API order)
        var totalCount = mappedBooks.Count;
        var items = mappedBooks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation(
            "Returning {ItemCount} books by author '{AuthorSlug}' out of {TotalCount}",
            items.Count, authorSlug, totalCount);

        return new PagedResult<BookDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling((double)totalCount / pageSize)
        );


    }

    private static BookDto MapToBookDto(
        FbBookListItemDto book,
        Dictionary<string, FbAuthorDto> authorsByName)
    {
        var authorNames = book.Author.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var authors = new List<AuthorDto>();
        foreach (var name in authorNames)
        {
            if (authorsByName.TryGetValue(name, out var author))
            {
                authors.Add(new AuthorDto(
                    Slug: author.Slug,
                    Name: author.Name
                ));
            }
            else
            {
                authors.Add(new AuthorDto(
                    Slug: string.Empty,
                    Name: name
                ));
            }
        }

        return new BookDto(
            Slug: book.Slug,
            Title: book.Title ?? string.Empty,
            Description: string.Empty,
            Url: book.Url ?? string.Empty,
            Thumbnail: book.CoverThumb ?? string.Empty,
            Authors: authors,
            Kind: book.Kind,
            Genre: book.Genre,
            Epoch: book.Epoch
        );
    }
}