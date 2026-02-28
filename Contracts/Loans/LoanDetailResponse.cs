namespace LibraryApi.Contracts.Loans;

public class LoanDetailResponse
{
    public int Id { get; set; }

    public int MemberId { get; set; }
    public string MemberName { get; set; } = null!;

    public int StaffId { get; set; }
    public string StaffName { get; set; } = null!;

    public int BookCopyId { get; set; }
    public string Barcode { get; set; } = null!;

    public int BookId { get; set; }
    public string BookTitle { get; set; } = null!;

    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsActive { get; set; }

    public bool HasFine { get; set; }
}