namespace LibraryApi.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}