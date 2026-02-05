using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FreeBooksAPI.Api.Services;
using FreeBooksAPI.Api.Models.Dtos;
using FreeBooksAPI.Api.Models.External;

namespace FreeBooksAPI.Tests.Services;

public class BookServiceTests
{
    private readonly Mock<IFreeBooksClient> _mockClient;
    private readonly Mock<ILogger<BookService>> _mockLogger;
    private readonly BookService _service;

    public BookServiceTests()
    {
        _mockClient = new Mock<IFreeBooksClient>();
        _mockLogger = new Mock<ILogger<BookService>>();
        _service = new BookService(_mockClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetBooksAsync_ReturnsPagedResult_WithCorrectPagination()
    {
        // Arrange
        var books = new List<FbBookListItemDto>
        {
            new() { Slug = "book1", Title = "Book 1", Author = "Author 1", Url = "url1", CoverThumb = "thumb1" },
            new() { Slug = "book2", Title = "Book 2", Author = "Author 2", Url = "url2", CoverThumb = "thumb2" },
            new() { Slug = "book3", Title = "Book 3", Author = "Author 3", Url = "url3", CoverThumb = "thumb3" }
        };

        var authors = new List<FbAuthorDto>
        {
            new() { Slug = "author-1", Name = "Author 1" },
            new() { Slug = "author-2", Name = "Author 2" },
            new() { Slug = "author-3", Name = "Author 3" }
        };

        _mockClient.Setup(c => c.GetListOfBooksAsync()).ReturnsAsync(books);
        _mockClient.Setup(c => c.GetListOfAuthorsAsync()).ReturnsAsync(authors);

        // Act
        var result = await _service.GetBooksAsync(page: 1, pageSize: 2);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task GetBooksAsync_FiltersBy_Kind()
    {
        // Arrange
        var books = new List<FbBookListItemDto>
        {
            new() { Slug = "book1", Title = "Book 1", Author = "Author 1", Kind = "epika", Url = "url1", CoverThumb = "thumb1" },
            new() { Slug = "book2", Title = "Book 2", Author = "Author 2", Kind = "liryka", Url = "url2", CoverThumb = "thumb2" }
        };

        var authors = new List<FbAuthorDto>
        {
            new() { Slug = "author-1", Name = "Author 1" }
        };

        _mockClient.Setup(c => c.GetListOfBooksAsync()).ReturnsAsync(books);
        _mockClient.Setup(c => c.GetListOfAuthorsAsync()).ReturnsAsync(authors);

        // Act
        var result = await _service.GetBooksAsync(page: 1, pageSize: 10, kind: "epika");

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("book1", result.Items[0].Slug);
    }

    [Fact]
    public async Task GetBooksAsync_SortsBy_TitleAscending()
    {
        // Arrange
        var books = new List<FbBookListItemDto>
        {
            new() { Slug = "book3", Title = "C Book", Author = "Author", Url = "url", CoverThumb = "thumb" },
            new() { Slug = "book1", Title = "A Book", Author = "Author", Url = "url", CoverThumb = "thumb" },
            new() { Slug = "book2", Title = "B Book", Author = "Author", Url = "url", CoverThumb = "thumb" }
        };

        var authors = new List<FbAuthorDto>
        {
            new() { Slug = "author-1", Name = "Author" }
        };

        _mockClient.Setup(c => c.GetListOfBooksAsync()).ReturnsAsync(books);
        _mockClient.Setup(c => c.GetListOfAuthorsAsync()).ReturnsAsync(authors);

        // Act
        var result = await _service.GetBooksAsync(page: 1, pageSize: 10, sortBy: "title", order: "asc");

        // Assert
        Assert.Equal("A Book", result.Items[0].Title);
        Assert.Equal("B Book", result.Items[1].Title);
        Assert.Equal("C Book", result.Items[2].Title);
    }

    [Fact]
    public async Task GetBooksAsync_SortsBy_TitleDescending()
    {
        // Arrange
        var books = new List<FbBookListItemDto>
        {
            new() { Slug = "book1", Title = "A Book", Author = "Author", Url = "url", CoverThumb = "thumb" },
            new() { Slug = "book2", Title = "B Book", Author = "Author", Url = "url", CoverThumb = "thumb" },
            new() { Slug = "book3", Title = "C Book", Author = "Author", Url = "url", CoverThumb = "thumb" }
        };

        var authors = new List<FbAuthorDto>
        {
            new() { Slug = "author-1", Name = "Author" }
        };

        _mockClient.Setup(c => c.GetListOfBooksAsync()).ReturnsAsync(books);
        _mockClient.Setup(c => c.GetListOfAuthorsAsync()).ReturnsAsync(authors);

        // Act
        var result = await _service.GetBooksAsync(page: 1, pageSize: 10, sortBy: "title", order: "desc");

        // Assert
        Assert.Equal("C Book", result.Items[0].Title);
        Assert.Equal("B Book", result.Items[1].Title);
        Assert.Equal("A Book", result.Items[2].Title);
    }

    [Fact]
    public async Task GetBookBySlugAsync_ReturnsCorrectBook()
    {
        // Arrange
        var bookDetails = new FbBookDetailsDto
        {
            Title = "Test Book",
            Url = "http://test.com",
            CoverThumb = "thumb.jpg",
            FragmentData = new FbFragmentData { Html = "Description" },
            Authors = new List<FbAuthorInfo>
            {
                new() { Slug = "test-author", Name = "Test Author" }
            },
            Kinds = new List<FbKindInfo> { new() { Name = "epika" } },
            Genres = new List<FbGenreInfo> { new() { Name = "powieść" } },
            Epochs = new List<FbEpochInfo> { new() { Name = "romantyzm" } }
        };

        _mockClient.Setup(c => c.GetBookBySlugAsync("test-book")).ReturnsAsync(bookDetails);

        // Act
        var result = await _service.GetBookBySlugAsync("test-book");

        // Assert
        Assert.Equal("test-book", result.Slug);
        Assert.Equal("Test Book", result.Title);
        Assert.Equal("", result.Description);
        Assert.Single(result.Authors);
        Assert.Equal("Test Author", result.Authors[0].Name);
    }
}