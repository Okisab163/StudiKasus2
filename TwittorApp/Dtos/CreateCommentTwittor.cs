namespace TwittorApp.Dtos
{
    public record CreateCommentTwittor
     (
        string CommentContent,
        int UserId,
        int TwittorId
     );
}
