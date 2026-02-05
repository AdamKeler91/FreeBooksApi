
using FreeBooksAPI.Api.Models.External;

public interface IFreeBooksClient
{
    public Task<List<FbBookListItemDto>> GetListOfBooksAsync();
    public Task<FbBookDetailsDto> GetBookBySlugAsync(string slug);
    public Task<List<FbBookListItemDto>> GetBooksByAuthorAsync(string slug);
    public Task<List<FbAuthorDto>> GetListOfAuthorsAsync();

}