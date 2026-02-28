namespace LibraryApi.Contracts.Fines;

public class FineCalculateRequest
{
    // günlük gecikme ücreti (istersen sabit de yaparız)
    public decimal DailyRate { get; set; } = 5m;

    // “create/update yap” gibi davranmak için
    public bool Persist { get; set; } = true;

    // return edilmediyse “şu tarihe göre hesapla” (null => DateTime.UtcNow)
    public DateTime? AsOf { get; set; }
}