using System;

namespace TwittorApp.Dtos
{
    public record TwittorInput
    (
        int? TwittorId,
        string TwittorContent,
        DateTime TwittorCreated,
        int UserId
    );
}
