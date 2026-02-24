namespace LibraryApi.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ISBN { get; set; }
    public int? PublishYear { get; set; }
    public int? PageCount { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}