namespace LibraryApi.Contracts.Loans;

public class LoanReturnRequest
{
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
}