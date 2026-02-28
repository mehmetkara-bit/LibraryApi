namespace LibraryApi.Contracts.Loans;

public class LoanBorrowRequest
{
    public int MemberId { get; set; }
    public int BookCopyId { get; set; }
    public int StaffId { get; set; } // staff kullanıcıdan çıkaracağız ileride, şimdilik request ile
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
}