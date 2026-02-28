using LibraryApi.Contracts.BookCopies;

namespace LibraryApi.Infrastructure.Catalog;

public interface IBookCopyService
{
    Task<List<BookCopyResponse>> CreateCopiesAsync(int bookId, BookCopyCreateRequest request);
    Task<List<BookCopyResponse>> GetCopiesByBookIdAsync(int bookId);
}