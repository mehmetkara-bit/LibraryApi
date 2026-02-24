namespace LibraryApi.Domain.Entities;

public class Loan
{
    public int Id { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public int BookCopyId { get; set; }
    public BookCopy BookCopy { get; set; } = null!;

    public int StaffId { get; set; }
    public Staff Staff { get; set; } = null!;

    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    // SQLite için “aktif loan” kuralı: aktifken 1, iade olunca null
    public int? ActiveMarker { get; set; } = 1;

    public Fine? Fine { get; set; }
}