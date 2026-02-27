public class BookCreateRequest
{
    public string Title { get; set; } = null!;
    public string? ISBN { get; set; }
    public int? PublishYear { get; set; }
    public int? PageCount { get; set; }

    public int CategoryId { get; set; }

    public List<int> AuthorIds { get; set; } = new();
}