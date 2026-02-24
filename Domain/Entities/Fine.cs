namespace LibraryApi.Domain.Entities;

public class Fine
{
    public int Id { get; set; }

    public int LoanId { get; set; }
    public Loan Loan { get; set; } = null!;

    public decimal Amount { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}