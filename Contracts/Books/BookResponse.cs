public class BookResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ISBN { get; set; }
    public int? PublishYear { get; set; }
    public int? PageCount { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;

    public List<string> Authors { get; set; } = new();

    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}