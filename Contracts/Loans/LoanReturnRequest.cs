namespace LibraryApi.Contracts.Loans;

public class LoanReturnRequest
{
    public int StaffId { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
}