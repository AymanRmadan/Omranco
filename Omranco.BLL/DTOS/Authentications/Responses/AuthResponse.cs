namespace Omranco.BLL.DTOS.Authentications.Responses
{
    public record AuthResponse(
        string Id,
        string? Email,
        string Name,
         string Token,
         int ExpireIn,
          string RefreshToken,
         DateTime RefreshTokenExpiration
        );

}
