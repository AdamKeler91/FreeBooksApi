using System.Text.Json.Serialization;

namespace FreeBooksAPI.Api.Models.External;

public class FbBookListItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("cover_thumb")]
    public string CoverThumb { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Epoch { get; set; } = string.Empty;
}