# FreeBooksAPI

REST API providing access to books and authors from [Wolne Lektury](https://wolnelektury.pl/api/) with additional filtering, sorting, and pagination capabilities.

## Features

- Browse books with filtering by kind, genre, and epoch
- Browse authors
- Search for books by specific author
- Pagination support on all endpoints
- Sorting by title, author name
- Caching for improved performance
- Swagger/OpenAPI documentation

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher

## Getting Started

### 1. Clone the repository

```bash
git clone <https://github.com/AdamKeler91/FreeBooksApi.git>
cd FreeBooksAPI
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Run the application

```bash
cd FreeBooksAPI.Api
dotnet run
```

The API will be available at:

- **HTTPS**: `https://localhost:5111`
- **HTTP**: `http://localhost:5111`
- **Swagger UI**: `https://localhost:5111/swagger`

## API Endpoints

### Books

#### Get all books (with filters and sorting)

```http
GET /api/books?page=1&pageSize=20&kind=epika&genre=powieść&epoch=romantyzm&sortBy=title&order=asc
```

**Query Parameters:**
| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `page` | integer | No | Page number (default: 1) | `1` |
| `pageSize` | integer | No | Items per page (default: 20, max: 100) | `20` |
| `kind` | string | No | Filter by book kind | `epika` |
| `genre` | string | No | Filter by genre | `powieść` |
| `epoch` | string | No | Filter by epoch | `romantyzm` |
| `sortBy` | string | No | Sort field: `title` or `author` | `title` |
| `order` | string | No | Sort order: `asc` or `desc` (default: `asc`) | `desc` |

**Example Request:**

```bash
curl "https://localhost:5111/api/books?kind=epika&sortBy=title&order=desc"
```

#### Get single book by slug

```http
GET /api/books/{slug}
```

**Example:**

```bash
curl "https://localhost:5111/api/books/pan-tadeusz"
```

### Authors

#### Get all authors (with sorting)

```http
GET /api/authors?page=1&pageSize=20&sortBy=name&order=asc
```

**Query Parameters:**
| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `page` | integer | No | Page number (default: 1) | `1` |
| `pageSize` | integer | No | Items per page (default: 20, max: 100) | `20` |
| `sortBy` | string | No | Sort field: `name` | `name` |
| `order` | string | No | Sort order: `asc` or `desc` (default: `asc`) | `desc` |

**Example Request:**

```bash
curl "https://localhost:5111/api/authors?sortBy=name&order=asc"
```

#### Get books by author

```http
GET /api/authors/{slug}/books?page=1&pageSize=20
```

**Example:**

```bash
curl "https://localhost:5111/api/authors/adam-mickiewicz/books"
```

## Response Format

##Book Description Field

Note about description field

The Wolne Lektury API does not provide a clear or consistent book description field in the books list endpoint.
As a result:

In the list of books, the description field is returned as an empty string.

In the single book details endpoint, the API also does not expose a dedicated description field — only metadata and content fragments.

The description field is therefore included in the API response to comply with the task requirements and to keep the contract future-proof, but its value may be empty until such data becomes available in the upstream API.

### Book Response

```json
{
  "slug": "pan-tadeusz",
  "title": "Pan Tadeusz",
  "description": "Description text...",
  "url": "https://wolnelektury.pl/katalog/lektura/pan-tadeusz/",
  "thumbnail": "https://wolnelektury.pl/media/book/cover_thumb/pan-tadeusz.jpg",
  "authors": [
    {
      "slug": "adam-mickiewicz",
      "name": "Adam Mickiewicz"
    }
  ],
  "kind": "Epika",
  "genre": "poemat",
  "epoch": "Romantyzm"
}
```

### Author Response

```json
{
  "slug": "adam-mickiewicz",
  "name": "Adam Mickiewicz"
}
```

### Paginated Response

```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

## Testing

### Run unit tests

```bash
cd FreeBooksAPI.Tests
dotnet test
```

### Manual testing with Swagger

1. Run the application: `dotnet run`
2. Open browser: `https://localhost:5111/swagger`
3. Try out endpoints interactively

### Manual testing with curl

**Get books:**

```bash
curl "https://localhost:5111/api/books?page=1&pageSize=5"
```

**Filter books:**

```bash
curl "https://localhost:5111/api/books?kind=epika&genre=powieść"
```

**Sort books:**

```bash
curl "https://localhost:5111/api/books?sortBy=title&order=desc"
```

**Get single book:**

```bash
curl "https://localhost:5111/api/books/pan-tadeusz"
```

**Get authors:**

```bash
curl "https://localhost:5111/api/authors?page=1&pageSize=10"
```

**Get books by author:**

```bash
curl "https://localhost:5111/api/authors/adam-mickiewicz/books"
```

## Project Structure

```
FreeBooksAPI/
├── FreeBooksAPI.Api/
│   ├── Controllers/          # API endpoints
│   ├── Services/             # Business logic
│   ├── Models/
│   │   ├── Dtos/            # Data Transfer Objects (API responses)
│   │   └── External/        # External API models
│   ├── Extensions/          # DI configuration
│   └── Program.cs           # Application entry point
└── FreeBooksAPI.Tests/
    └── Services/            # Unit tests
```

## Architecture

- **Controllers**: Handle HTTP requests, validation, and responses
- **Services**: Business logic (filtering, sorting, pagination, mapping)
- **HttpClient**: Communication with Wolne Lektury API
- **Caching**: In-memory caching for improved performance
- **Dependency Injection**: Clean architecture with testable components

## Technologies

- **.NET 8.0**
- **ASP.NET Core Web API**
- **Swagger/OpenAPI** for documentation
- **xUnit** for testing
- **Moq** for mocking in tests
- **System.Text.Json** for JSON serialization

## Performance Optimizations

- **In-memory caching** of API responses (configurable TTL)
- **Async/await** throughout for non-blocking I/O
- **Efficient LINQ** queries for filtering and sorting

## Error Handling

The API returns appropriate HTTP status codes:

- `200 OK` - Successful request
- `400 Bad Request` - Invalid parameters
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## License

This project is for educational/recruitment purposes.

## Contact

For questions or issues, please open an issue in the repository.
