using LibraryApi.Contracts.BookCopies;
using LibraryApi.Domain.Entities;
using LibraryApi.Domain.Enums;
using LibraryApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Catalog;

public class BookCopyService : IBookCopyService
{
    private readonly AppDbContext _db;

    public BookCopyService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<BookCopyResponse>> CreateCopiesAsync(int bookId, BookCopyCreateRequest request)
    {
        if (request.Count <= 0 || request.Count > 50)
            throw new InvalidOperationException("Count must be between 1 and 50.");

        var bookExists = await _db.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists)
            throw new InvalidOperationException("Book not found.");

        var copies = new List<BookCopy>(request.Count);

        for (int i = 0; i < request.Count; i++)
        {
            copies.Add(new BookCopy
            {
                BookId = bookId,
                Barcode = Guid.NewGuid().ToString("N"), // unique
                Status = BookCopyStatus.Available,
                ShelfLocation = request.ShelfLocation
            });
        }

        _db.BookCopies.AddRange(copies);
        await _db.SaveChangesAsync();

        return copies.Select(ToResponse).ToList();
    }

    public async Task<List<BookCopyResponse>> GetCopiesByBookIdAsync(int bookId)
    {
        var exists = await _db.Books.AnyAsync(b => b.Id == bookId);
        if (!exists)
            throw new InvalidOperationException("Book not found.");

        var copies = await _db.BookCopies
            .AsNoTracking()
            .Where(c => c.BookId == bookId)
            .OrderByDescending(c => c.CreatedDate)
            .Select(c => new BookCopyResponse
            {
                Id = c.Id,
                BookId = c.BookId,
                Barcode = c.Barcode,
                Status = c.Status,
                ShelfLocation = c.ShelfLocation,
                CreatedDate = c.CreatedDate
            })
            .ToListAsync();

        return copies;
    }

    private static BookCopyResponse ToResponse(BookCopy c) => new()
    {
        Id = c.Id,
        BookId = c.BookId,
        Barcode = c.Barcode,
        Status = c.Status,
        ShelfLocation = c.ShelfLocation,
        CreatedDate = c.CreatedDate
    };
}