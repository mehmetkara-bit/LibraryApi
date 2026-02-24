using LibraryApi.Domain.Enums;

namespace LibraryApi.Domain.Entities;

public class BookCopy
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public string Barcode { get; set; } = null!;
    public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;

    public string? ShelfLocation { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}