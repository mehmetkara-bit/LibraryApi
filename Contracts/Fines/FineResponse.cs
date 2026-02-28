namespace LibraryApi.Contracts.Fines;

public class FineResponse
{
    public int Id { get; set; }
    public int LoanId { get; set; }

    public decimal Amount { get; set; }
    public int DaysLate { get; set; }

    public bool IsPaid { get; set; }
    public DateTime CreatedDate { get; set; }
}