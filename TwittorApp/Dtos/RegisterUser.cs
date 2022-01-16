using System;

namespace TwittorApp.Dtos
{
    public record RegisterUser
    (
        int? UserId,
        string FullName,
        string Email,
        string UserName,
        string Password,
        bool? IsBanned,
        DateTime? UserCreated
    );
}
