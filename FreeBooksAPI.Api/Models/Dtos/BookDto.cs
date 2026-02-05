namespace FreeBooksAPI.Api.Models.Dtos;

public record BookDto(
    string Slug,
    string Title,
    string? Description,
    string Url,
    string Thumbnail,
    List<AuthorDto> Authors,
    string? Kind,
    string? Genre,
    string? Epoch
);