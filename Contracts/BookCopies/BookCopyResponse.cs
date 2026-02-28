using LibraryApi.Domain.Enums;

namespace LibraryApi.Contracts.BookCopies;

public class BookCopyResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string Barcode { get; set; } = null!;
    public BookCopyStatus Status { get; set; }
    public string? ShelfLocation { get; set; }
    public DateTime CreatedDate { get; set; }
}