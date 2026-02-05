using Microsoft.AspNetCore.Mvc;
using FreeBooksAPI.Api.Services;
using FreeBooksAPI.Api.Models.Dtos;

namespace FreeBooksAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;
    private readonly ILogger<AuthorsController> _logger;

    public AuthorsController(
        IAuthorService authorService,
        ILogger<AuthorsController> logger)
    {
        _authorService = authorService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of authors with optional sorting.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field: 'name' (default: 'name')</param>
    /// <param name="order">Sort order: 'asc' or 'desc' (default: 'asc')</param>
    /// <remarks>
    /// Sample requests:
    /// 
    ///     GET /api/authors
    ///     GET /api/authors?page=2&amp;pageSize=10
    ///     GET /api/authors?sortBy=name&amp;order=desc
    /// 
    /// </remarks>
    /// <response code="200">Returns paginated list of authors</response>
    /// <response code="400">If parameters are invalid</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuthorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<AuthorDto>>> GetAuthors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? order = null)
    {
        // Validation
        if (page < 1)
            return BadRequest("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("PageSize must be between 1 and 100");

        if (sortBy != null && !sortBy.Equals("name", StringComparison.OrdinalIgnoreCase))
            return BadRequest("sortBy must be 'name'");

        if (order != null &&
            !order.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
            !order.Equals("desc", StringComparison.OrdinalIgnoreCase))
            return BadRequest("order must be 'asc' or 'desc'");

        try
        {
            var result = await _authorService.GetAuthorsAsync(page, pageSize, sortBy, order);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error fetching authors");
            return StatusCode(500, "Failed to fetch authors. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching authors");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Retrieves books by a specific author.
    /// </summary>
    /// <param name="slug">Author's slug (ID)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <remarks>
    /// Sample requests:
    /// 
    ///     GET /api/authors/adam-mickiewicz/books
    ///     GET /api/authors/adam-mickiewicz/books?page=2&amp;pageSize=10
    /// 
    /// </remarks>
    /// <response code="200">Returns paginated list of books by the author</response>
    /// <response code="400">If parameters are invalid</response>
    /// <response code="404">If author not found</response>
    [HttpGet("{slug}/books")]
    [ProducesResponseType(typeof(PagedResult<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<BookDto>>> GetBooksByAuthor(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest("Author slug is required");

        if (page < 1)
            return BadRequest("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("PageSize must be between 1 and 100");

        try
        {
            var result = await _authorService.GetBooksByAuthorAsync(slug, page, pageSize);

            if (result.TotalCount == 0)
                return NotFound($"No books found for author '{slug}'");

            return Ok(result);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Author '{Slug}' not found", slug);
            return NotFound($"Author '{slug}' not found");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error fetching books for author '{Slug}'", slug);
            return StatusCode(500, "Failed to fetch books. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching books for author '{Slug}'", slug);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
