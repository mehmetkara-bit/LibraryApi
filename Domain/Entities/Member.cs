namespace LibraryApi.Domain.Entities;

public class Member
{
    // Shared PK (PK = FK -> Users.Id)
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}