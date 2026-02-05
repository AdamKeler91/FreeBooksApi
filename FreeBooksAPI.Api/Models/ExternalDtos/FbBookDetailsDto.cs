using System.Text.Json.Serialization;

namespace FreeBooksAPI.Api.Models.External;

public class FbBookDetailsDto
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("cover_thumb")]
    public string CoverThumb { get; set; } = string.Empty;

    [JsonPropertyName("fragment_data")]
    public FbFragmentData? FragmentData { get; set; }

    public List<FbAuthorInfo> Authors { get; set; } = new();

    public List<FbKindInfo> Kinds { get; set; } = new();
    public List<FbGenreInfo> Genres { get; set; } = new();
    public List<FbEpochInfo> Epochs { get; set; } = new();
}

public class FbFragmentData
{
    public string Html { get; set; } = string.Empty;
}

public class FbAuthorInfo
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class FbKindInfo
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class FbGenreInfo
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class FbEpochInfo
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}