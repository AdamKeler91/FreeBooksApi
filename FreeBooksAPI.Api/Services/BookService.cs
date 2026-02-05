using FreeBooksAPI.Api.Models.Dtos;
using FreeBooksAPI.Api.Models.External;

namespace FreeBooksAPI.Api.Services;

public class BookService : IBookService
{
    private readonly IFreeBooksClient _freeBooksClient;
    private readonly ILogger<BookService> _logger;

    public BookService(
        IFreeBooksClient freeBooksClient,
        ILogger<BookService> logger)
    {
        _freeBooksClient = freeBooksClient;
        _logger = logger;
    }

    public async Task<BookDto> GetBookBySlugAsync(string slug)
    {
        var book = await _freeBooksClient.GetBookByIdAsync(slug);
        return new BookDto(
        Slug: slug,
        Title: book.Title,
        Description: book.FragmentData?.Html ?? string.Empty,
        Url: book.Url,
        Thumbnail: book.CoverThumb,
        Authors: book.Authors.Select(a => new AuthorDto(
            Slug: a.Slug,
            Name: a.Name
        )).ToList()
    );
    }

    public async Task<PagedResult<BookDto>> GetBooksAsync(
        int page,
        int pageSize,
        string? kind = null,
        string? genre = null,
        string? epoch = null,
        string? sortBy = null,
        string? order = null)
    {
        _logger.LogInformation("Fetching books list");


        var books = await _freeBooksClient.GetListOfBooksAsync();

        var authors = await _freeBooksClient.GetListOfAuthorsAsync();

        var authorsByName = authors
            .ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);


        var mappedBooks = books.Select(b => MapToBookDto(b, authorsByName))
                                .ToList();


        var totalCount = mappedBooks.Count;

        var items = mappedBooks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

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
        return new BookDto(
            Slug: book.Slug,
            Title: book.Title,
            Description: null,
            Url: book.Url,
            Thumbnail: book.CoverThumb,
            Authors: MapAuthors(book.Author, authorsByName)
        );
    }

    private static List<AuthorDto> MapAuthors(
        string authorName,
        Dictionary<string, FbAuthorDto> authorsByName)
    {

        var names = authorName.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var result = new List<AuthorDto>();

        foreach (var name in names)
        {
            if (authorsByName.TryGetValue(name, out var author))
            {
                result.Add(new AuthorDto(
                    Slug: author.Slug,
                    Name: author.Name
                ));
            }
            else
            {
                result.Add(new AuthorDto(
                    Slug: null,
                    Name: name
                ));
            }
        }

        return result;
    }
}
