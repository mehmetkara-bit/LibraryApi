namespace LibraryApi.Contracts.BookCopies;

public class BookCopyCreateRequest
{
    public int Count { get; set; } = 1; // kaç kopya üretilecek
    public string? ShelfLocation { get; set; } // optional
}