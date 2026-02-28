namespace LibraryApi.Contracts.Loans;

public class LoanResponse
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int BookCopyId { get; set; }
    public int StaffId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsActive { get; set; }
}