namespace TwittorApp.Dtos
{
    public record UpdateUserPassword
     (
         string UserName,
         string NewPassword,
         string OldPassword
     );
}
