using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FreeBooksAPI.Api.Services;
using FreeBooksAPI.Api.Models.Dtos;
using FreeBooksAPI.Api.Models.External;

namespace FreeBooksAPI.Tests.Services;

public class AuthorServiceTests
{
    private readonly Mock<IFreeBooksClient> _mockClient;
    private readonly Mock<ILogger<AuthorService>> _mockLogger;
    private readonly AuthorService _service;

    public AuthorServiceTests()
    {
        _mockClient = new Mock<IFreeBooksClient>();
        _mockLogger = new Mock<ILogger<AuthorService>>();
        _service = new AuthorService(_mockClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAuthorsAsync_ReturnsPagedResult()
    {
        // Arrange
        var authors = new List<FbAuthorDto>
        {
            new() { Slug = "author1", Name = "Author 1" },
            new() { Slug = "author2", Name = "Author 2" },
            new() { Slug = "author3", Name = "Author 3" }
        };

        _mockClient.Setup(c => c.GetListOfAuthorsAsync()).ReturnsAsync(authors);

        // Act
        var result = await _service.GetAuthorsAsync(page: 1, pageSize: 2);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAuthorsAsync_SortsBy_NameAscending()
    {
        // Arrange
        var authors = new List<FbAuthorDto>
        {
            new() { Slug = "c", Name = "Charlie" },
            new() { Slug = "a", Name = "Alice" },
            new() { Slug = "b", Name = "Bob" }
        };

        _mockClient.Setup(c => c.GetListOfAuthorsAsync()).ReturnsAsync(authors);

        // Act
        var result = await _service.GetAuthorsAsync(page: 1, pageSize: 10, sortBy: "name", order: "asc");

        // Assert
        Assert.Equal("Alice", result.Items[0].Name);
        Assert.Equal("Bob", result.Items[1].Name);
        Assert.Equal("Charlie", result.Items[2].Name);
    }

    [Fact]
    public async Task GetBooksByAuthorAsync_ReturnsBooksForAuthor()
    {
        // Arrange
        var books = new List<FbBookListItemDto>
        {
            new() { Slug = "book1", Title = "Book 1", Author = "Test Author", Url = "url", CoverThumb = "thumb" },
            new() { Slug = "book2", Title = "Book 2", Author = "Test Author", Url = "url", CoverThumb = "thumb" }
        };

        var authors = new List<FbAuthorDto>
        {
            new() { Slug = "test-author", Name = "Test Author" }
        };

        _mockClient.Setup(c => c.GetBooksByAuthorAsync("test-author")).ReturnsAsync(books);
        _mockClient.Setup(c => c.GetListOfAuthorsAsync()).ReturnsAsync(authors);

        // Act
        var result = await _service.GetBooksByAuthorAsync("test-author", page: 1, pageSize: 10);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Book 1", result.Items[0].Title);
    }
}