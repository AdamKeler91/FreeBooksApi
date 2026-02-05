using System.Text.Json;
using FreeBooksAPI.Api.Models.External;
using Microsoft.Extensions.Caching.Memory;

namespace FreeBooksAPI.Api.Services;

public class FreeBooksClient : IFreeBooksClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FreeBooksClient> _logger;

    public FreeBooksClient(HttpClient httpClient, IMemoryCache cache, ILogger<FreeBooksClient> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }



    public async Task<FbBookDetailsDto> GetBookByIdAsync(string slug)
    {
        var cacheKey = $"book_{slug}";
        string endpoint = $"books/{slug}/";

        try
        {
            if (_cache.TryGetValue(cacheKey, out FbBookDetailsDto? cachedBook))
            {
                _logger.LogInformation("Retrieved book '{Slug}' from cache", slug);
                return cachedBook!;
            }


            _logger.LogInformation("Fetching book '{Slug}' from API", slug);

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var book = await response.Content.ReadFromJsonAsync<FbBookDetailsDto>()
                ?? throw new InvalidOperationException($"Book '{slug}' not found");


            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(15)
            };
            _cache.Set(cacheKey, book, cacheOptions);

            _logger.LogInformation("Successfully cached book '{Slug}'", slug);

            return book!;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching book '{Slug}'", slug);
            throw new InvalidOperationException($"Failed to fetch book '{slug}': {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error for book '{Slug}'", slug);
            throw new InvalidOperationException($"Failed to deserialize book '{slug}': {ex.Message}", ex);
        }
    }

    public async Task<List<FbBookListItemDto>> GetBooksByAuthorAsync(string slug)
    {
        var cacheKey = $"books_by_{slug}";
        var endpoint = $"authors/{slug}/books/";

        try
        {

            if (_cache.TryGetValue(cacheKey, out List<FbBookListItemDto>? cachedBooks))
            {
                _logger.LogInformation("Retrieved books by author '{AuthorSlug}' from cache", slug);
                return cachedBooks!;
            }

            // 2. Make HTTP request
            _logger.LogInformation("Fetching books by author '{AuthorSlug}' from API: {Endpoint}", slug, endpoint);

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            // 3. Deserialize
            var books = await response.Content.ReadFromJsonAsync<List<FbBookListItemDto>>()
                ?? new List<FbBookListItemDto>();

            // 4. Cache the result
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            _cache.Set(cacheKey, books, cacheOptions);

            _logger.LogInformation("Successfully fetched and cached {Count} books by author '{AuthorSlug}'", books.Count, slug);

            return books;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching books by author '{AuthorSlug}' from {Endpoint}", slug, endpoint);
            throw new InvalidOperationException($"Failed to fetch books by author '{slug}' from API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for books by author '{AuthorSlug}' from endpoint {Endpoint}", slug, endpoint);
            throw new InvalidOperationException($"Failed to deserialize books by author '{slug}': {ex.Message}", ex);
        }

    }

    public async Task<List<FbAuthorDto>> GetListOfAuthorsAsync()
    {
        const string cacheKey = "all_authors";
        const string endpoint = "authors/";

        try
        {

            if (_cache.TryGetValue(cacheKey, out List<FbAuthorDto>? cachedAuthors))
            {
                _logger.LogInformation("Retrieved {Count} authors from cache", cachedAuthors!.Count);
                return cachedAuthors;
            }


            _logger.LogInformation("Fetching authors from Wolne Lektury API: {Endpoint}", endpoint);

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();


            var authors = await response.Content.ReadFromJsonAsync<List<FbAuthorDto>>()
                ?? new List<FbAuthorDto>();

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(15)
            };
            _cache.Set(cacheKey, authors, cacheOptions);

            _logger.LogInformation("Successfully fetched and cached {Count} authors", authors.Count);

            return authors;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching authors from {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to fetch authors from Wolne Lektury API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for endpoint {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to deserialize authors response: {ex.Message}", ex);
        }
    }

    public async Task<List<FbBookListItemDto>> GetListOfBooksAsync()
    {
        const string cacheKey = "all_books";
        const string endpoint = "books/";

        try
        {

            if (_cache.TryGetValue(cacheKey, out List<FbBookListItemDto>? cachedBooks))
            {
                _logger.LogInformation("Retrieved {Count} books from cache", cachedBooks!.Count);
                return cachedBooks;
            }


            _logger.LogInformation("Fetching books from Wolne Lektury API: {Endpoint}", endpoint);

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();


            var books = await response.Content.ReadFromJsonAsync<List<FbBookListItemDto>>()
                ?? new List<FbBookListItemDto>();

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
            _cache.Set(cacheKey, books, cacheOptions);

            _logger.LogInformation("Successfully fetched and cached {Count} books", books.Count);

            return books;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching books from {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to fetch books from Wolne Lektury API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for endpoint {Endpoint}", endpoint);
            throw new InvalidOperationException($"Failed to deserialize books response: {ex.Message}", ex);
        }
    }
}