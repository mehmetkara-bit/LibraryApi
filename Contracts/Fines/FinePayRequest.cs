namespace LibraryApi.Contracts.Fines;

public class FinePayRequest
{
    public DateTime PaidDate { get; set; } = DateTime.UtcNow;
}