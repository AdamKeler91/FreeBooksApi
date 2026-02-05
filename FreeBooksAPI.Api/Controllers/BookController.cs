using Microsoft.AspNetCore.Mvc;
using FreeBooksAPI.Api.Services;
using FreeBooksAPI.Api.Models.Dtos;

namespace FreeBooksAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(
        IBookService bookService,
        ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of books with optional filters and pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="kind">Filter by book kind. Example: 'epika', 'liryka'</param>
    /// <param name="genre">Filter by genre. Example: 'powieść', 'dramat'</param>
    /// <param name="epoch">Filter by epoch. Example: 'romantyzm', 'pozytywizm'</param>
    /// <param name="sortBy">Sort field: 'title' or 'author' (default: 'title')</param>
    /// <param name="order">Sort order: 'asc' (ascending) or 'desc' (descending). Default: 'asc'</param>
    /// <remarks>
    /// Sample requests:
    /// 
    ///     GET /api/books
    ///     GET /api/books?page=2&amp;pageSize=10
    ///     GET /api/books?kind=epika&amp;sortBy=title&amp;order=desc
    ///     GET /api/books?genre=powieść&amp;epoch=romantyzm
    /// 
    /// </remarks>
    /// <response code="200">Returns paginated list of books</response>
    /// <response code="400">If parameters are invalid</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<BookDto>>> GetBooks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? kind = null,
        [FromQuery] string? genre = null,
        [FromQuery] string? epoch = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? order = null)
    {
        // Validation
        if (page < 1)
            return BadRequest("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("PageSize must be between 1 and 100");

        // Validate sortBy
        if (sortBy != null &&
            !sortBy.Equals("title", StringComparison.OrdinalIgnoreCase) &&
            !sortBy.Equals("author", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("sortBy must be 'title' or 'author'");
        }

        // Validate order
        if (order != null &&
            !order.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
            !order.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("order must be 'asc' or 'desc'");
        }

        try
        {
            var result = await _bookService.GetBooksAsync(
                page, pageSize, kind, genre, epoch, sortBy, order);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error fetching books");
            return StatusCode(500, "Failed to fetch books. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching books");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Retrieves a single book by its slug (ID).
    /// </summary>
    /// <param name="slug">Book's slug (e.g., 'pan-tadeusz')</param>
    /// <remarks>
    /// Sample requests:
    /// 
    ///     GET /api/books/pan-tadeusz
    ///     GET /api/books/studnia-i-wahadlo
    /// 
    /// </remarks>
    /// <response code="200">Returns the book details</response>
    /// <response code="400">If slug is invalid</response>
    /// <response code="404">If book not found</response>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> GetBookBySlug(string slug)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest("Book slug is required");

        try
        {
            var book = await _bookService.GetBookBySlugAsync(slug);
            return Ok(book);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Book '{Slug}' not found", slug);
            return NotFound($"Book '{slug}' not found");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error fetching book '{Slug}'", slug);
            return StatusCode(500, "Failed to fetch book. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching book '{Slug}'", slug);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}