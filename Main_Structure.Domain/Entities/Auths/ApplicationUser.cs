namespace Main_Structure.Domain.Entities.Auths
{
    public sealed class ApplicationUser : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;
        public List<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
