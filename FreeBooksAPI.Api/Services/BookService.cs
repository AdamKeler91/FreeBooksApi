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
        return MapBookDetailsToBookDto(book, slug);
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


        // 1. Fetch data
        var books = await _freeBooksClient.GetListOfBooksAsync();
        var authors = await _freeBooksClient.GetListOfAuthorsAsync();

        var authorsByName = authors.ToDictionary(
            a => a.Name,
            StringComparer.OrdinalIgnoreCase);

        // 2. Map to DTOs
        var mappedBooks = books
            .Select(b => MapListItemToBookDto(b, authorsByName))
            .ToList();

        IEnumerable<BookDto> filteredBooks = mappedBooks;

        // 3. Filter
        if (!string.IsNullOrWhiteSpace(kind))
            filteredBooks = filteredBooks.Where(b =>
                b.Kind != null && b.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(genre))
            filteredBooks = filteredBooks.Where(b =>
                b.Genre != null && b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(epoch))
            filteredBooks = filteredBooks.Where(b =>
                b.Epoch != null && b.Epoch.Equals(epoch, StringComparison.OrdinalIgnoreCase));

        // 4. Sort
        var sortByLower = sortBy?.ToLower() ?? "title";
        var orderLower = order?.ToLower() ?? "asc";

        if (sortByLower == "author")
        {
            if (orderLower == "desc")
                filteredBooks = filteredBooks.OrderByDescending(b =>
                    b.Authors.FirstOrDefault()?.Name ?? string.Empty);
            else
                filteredBooks = filteredBooks.OrderBy(b =>
                    b.Authors.FirstOrDefault()?.Name ?? string.Empty);
        }
        else // title or default
        {
            if (orderLower == "desc")
                filteredBooks = filteredBooks.OrderByDescending(b => b.Title);
            else
                filteredBooks = filteredBooks.OrderBy(b => b.Title);
        }

        // 5. Paginate
        var totalCount = filteredBooks.Count();
        var items = filteredBooks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation(
            "Returning {ItemCount} books out of {TotalCount}",
            items.Count, totalCount);

        return new PagedResult<BookDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: (int)Math.Ceiling((double)totalCount / pageSize));
    }



    private static BookDto MapListItemToBookDto(
    FbBookListItemDto book,
    Dictionary<string, FbAuthorDto> authorsByName)
    {
        return new BookDto(
            Slug: book.Slug,
            Title: book.Title,
            Description: string.Empty,
            Url: book.Url,
            Thumbnail: book.CoverThumb,
            Authors: MapAuthors(book.Author, authorsByName),
            Kind: book.Kind,
            Genre: book.Genre,
            Epoch: book.Epoch
        );
    }

    private static BookDto MapBookDetailsToBookDto(FbBookDetailsDto book, string slug)
    {
        return new BookDto(
            Slug: slug,
            Title: book.Title,
            Description: string.Empty,
            Url: book.Url,
            Thumbnail: book.CoverThumb,
            Authors: book.Authors?.Select(a => new AuthorDto(

                Slug: a.Slug ?? string.Empty,
                Name: a.Name ?? string.Empty

            )).ToList() ?? new List<AuthorDto>(),
            Kind: book.Kinds?.FirstOrDefault()?.Name,
            Genre: book.Genres?.FirstOrDefault()?.Name,
            Epoch: book.Epochs?.FirstOrDefault()?.Name
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
                    Slug: string.Empty,
                    Name: name
                ));
            }
        }

        return result;
    }
}
