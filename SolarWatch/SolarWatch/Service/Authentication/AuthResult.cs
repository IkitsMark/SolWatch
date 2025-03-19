namespace SolarWatch.Service.Authentication;

public record AuthResult(
    bool Success,
    string Email,
    string UserName,
    string Token)
{
    public Dictionary<string, string> ErrorMessages { get; init; } = new();
}