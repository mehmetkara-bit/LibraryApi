using LibraryApi.Contracts.Books;

namespace LibraryApi.Infrastructure.Catalog;

public interface IBookService
{
    Task<List<BookResponse>> GetAllAsync(int? categoryId, string? q);
    Task<BookResponse?> GetByIdAsync(int id);
    Task<BookResponse> CreateAsync(BookCreateRequest request);
    Task<BookResponse?> UpdateAsync(int id, BookUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}