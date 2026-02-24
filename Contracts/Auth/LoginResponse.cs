namespace LibraryApi.Contracts.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
}