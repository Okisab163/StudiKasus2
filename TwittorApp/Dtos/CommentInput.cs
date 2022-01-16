using System;

namespace TwittorApp.Dtos
{
    public record CommentInput
    (
        int? CommentId,
        string CommentContent,
        DateTime CommentCreated,
        int TwittorId
    );
}
