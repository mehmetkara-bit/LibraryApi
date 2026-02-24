using LibraryApi.Domain.Enums;

namespace LibraryApi.Domain.Entities;

public class Staff
{
    // Shared PK (PK = FK -> Users.Id)
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public StaffRoleType RoleType { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}