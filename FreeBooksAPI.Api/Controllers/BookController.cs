using Microsoft.AspNetCore.Mvc;
using FreeBooksAPI.Api.Services;
using FreeBooksAPI.Api.Models.Dtos;

namespace FreeBooksAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Retrieves a list of books with optional filters and pagination.
    /// </summary>
    /// <param name="page">Page number (default is 1)</param>
    /// <param name="pageSize">Page size (default is 20)</param>
    /// <param name="kind">Kind of book</param>
    /// <param name="genre">Genre of book</param>
    /// <param name="epoch">Epoch</param>
    /// <param name="sortBy">Sort by field (e.g., title)</param>
    /// <param name="order">Sort order: asc / desc</param>
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookDto>>> GetBooks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? kind = null,
        [FromQuery] string? genre = null,
        [FromQuery] string? epoch = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? order = null)
    {
        try
        {
            var result = await _bookService.GetBooksAsync(
                page, pageSize, kind, genre, epoch, sortBy, order);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves a single book by its slug (ID).
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<ActionResult<BookDto>> GetBookBySlug(string slug)
    {
        try
        {
            var book = await _bookService.GetBookBySlugAsync(slug);
            if (book is null)
                return NotFound();

            return Ok(book);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
