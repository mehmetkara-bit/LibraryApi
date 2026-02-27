using LibraryApi.Contracts.Books;
using LibraryApi.Domain.Entities;
using LibraryApi.Domain.Enums;
using LibraryApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Catalog;

public class BookService : IBookService
{
    private readonly AppDbContext _db;

    public BookService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<BookResponse> CreateAsync(BookCreateRequest request)
    {
        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException("Title is required.");

        var isbn = request.ISBN?.Trim();
        if (isbn == "") isbn = null;

        // Category var mı?
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new InvalidOperationException("Category not found.");

        // ISBN unique mi? (unique index var)
        if (isbn is not null)
        {
            var isbnExists = await _db.Books.AnyAsync(b => b.ISBN == isbn);
            if (isbnExists)
                throw new InvalidOperationException("ISBN already exists.");
        }

        // AuthorIds normalize + varlık kontrolü
        var authorIds = request.AuthorIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (authorIds.Count > 0)
        {
            var existingCount = await _db.Authors
                .Where(a => authorIds.Contains(a.Id))
                .CountAsync();

            if (existingCount != authorIds.Count)
                throw new InvalidOperationException("One or more authors not found.");
        }

        var book = new Book
        {
            Title = title,
            ISBN = isbn,
            PublishYear = request.PublishYear,
            PageCount = request.PageCount,
            CategoryId = request.CategoryId
        };

        foreach (var authorId in authorIds)
        {
            book.BookAuthors.Add(new BookAuthor
            {
                AuthorId = authorId
            });
        }

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        var created = await GetByIdAsync(book.Id);
        return created!;
    }

    public async Task<BookResponse?> UpdateAsync(int id, BookUpdateRequest request)
    {
        // Join koleksiyonunu değiştireceğimiz için BookAuthors include ediyoruz
        var book = await _db.Books
            .Include(b => b.BookAuthors)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return null;

        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException("Title is required.");

        var isbn = request.ISBN?.Trim();
        if (isbn == "") isbn = null;

        // Category var mı?
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new InvalidOperationException("Category not found.");

        // ISBN unique mi? (bu kitap hariç)
        if (isbn is not null)
        {
            var isbnTaken = await _db.Books.AnyAsync(b => b.ISBN == isbn && b.Id != id);
            if (isbnTaken)
                throw new InvalidOperationException("ISBN already exists.");
        }

        // AuthorIds normalize + varlık kontrolü
        var authorIds = request.AuthorIds
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        if (authorIds.Count > 0)
        {
            var existingCount = await _db.Authors
                .Where(a => authorIds.Contains(a.Id))
                .CountAsync();

            if (existingCount != authorIds.Count)
                throw new InvalidOperationException("One or more authors not found.");
        }

        // Basit alanlar
        book.Title = title;
        book.ISBN = isbn;
        book.PublishYear = request.PublishYear;
        book.PageCount = request.PageCount;
        book.CategoryId = request.CategoryId;

        // Many-to-many update:
        // - request'te olmayan eski yazarları sil
        // - request'te olup eskide olmayanları ekle

        var currentAuthorIds = book.BookAuthors.Select(ba => ba.AuthorId).ToHashSet();

        var toRemove = book.BookAuthors
            .Where(ba => !authorIds.Contains(ba.AuthorId))
            .ToList();

        foreach (var ba in toRemove)
            book.BookAuthors.Remove(ba);

        var toAdd = authorIds
            .Where(aid => !currentAuthorIds.Contains(aid))
            .ToList();

        foreach (var aid in toAdd)
            book.BookAuthors.Add(new BookAuthor { AuthorId = aid });

        await _db.SaveChangesAsync();

        var updated = await GetByIdAsync(book.Id);
        return updated!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return false;

        // kopyası varsa silme
        if (book.Copies.Count > 0)
            throw new InvalidOperationException("Cannot delete a book that has copies.");

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<BookResponse?> GetByIdAsync(int id)
    {
        var book = await _db.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return null;
        return ToResponse(book);
    }

    public async Task<List<BookResponse>> GetAllAsync(int? categoryId, string? q)
    {
        var query = _db.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(b => b.Title.Contains(q));

        var books = await query
            .OrderBy(b => b.Title)
            .ToListAsync();

        return books.Select(ToResponse).ToList();
    }

    private static BookResponse ToResponse(Book b)
    {
        var totalCopies = b.Copies.Count;
        var availableCopies = b.Copies.Count(c => c.Status == BookCopyStatus.Available);

        var authors = b.BookAuthors
            .Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}")
            .ToList();

        return new BookResponse
        {
            Id = b.Id,
            Title = b.Title,
            ISBN = b.ISBN,
            PublishYear = b.PublishYear,
            PageCount = b.PageCount,
            CategoryId = b.CategoryId,
            CategoryName = b.Category.Name,
            Authors = authors,
            TotalCopies = totalCopies,
            AvailableCopies = availableCopies
        };
    }
}