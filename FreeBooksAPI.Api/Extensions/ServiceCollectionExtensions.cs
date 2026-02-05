using FreeBooksAPI.Api.Services;

namespace FreeBooksAPI.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IAuthorService, AuthorService>();

        // HttpClient for WolneLektury API
        services.AddHttpClient<IFreeBooksClient, FreeBooksClient>(client =>
        {
            client.BaseAddress = new Uri("https://wolnelektury.pl/api/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });


        // Memory cache for performance optimization
        services.AddMemoryCache();

        return services;
    }
}