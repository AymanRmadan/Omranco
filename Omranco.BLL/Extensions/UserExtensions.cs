namespace Omranco.BLL
{
    public static class UserExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userId, out var id) ? id : 0;
        }

    }
}
