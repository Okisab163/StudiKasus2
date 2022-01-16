namespace TwittorApp.Dtos
{
    public record TransactionStatus
    (
        bool IsSucceed,
        string? Message
    );
}
