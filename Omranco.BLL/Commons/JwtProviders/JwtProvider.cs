using Omranco.Domain.Entities.Auths;

namespace Omranco.BLL;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string token, int expiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims =
            new List<Claim>
            {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),

            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()
            ),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email!
            ),

            new(
                JwtRegisteredClaimNames.GivenName,
                user.Name
            ),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()
            )
            };


        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role
                )
            );
        }


        var symmetricSecurityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _options.Key
                )
            );


        var signingCredentials =
            new SigningCredentials(
                symmetricSecurityKey,
                SecurityAlgorithms.HmacSha256
            );


        var token =
            new JwtSecurityToken(
                issuer:
                    _options.Issuer,

                audience:
                    _options.Audience,

                claims:
                    claims,

                expires:
                    DateTime.UtcNow.AddMinutes(
                        _options.ExpiryMinutes
                    ),

                signingCredentials:
                    signingCredentials
            );


        return (
            token:
                new JwtSecurityTokenHandler()
                    .WriteToken(token),

            expiresIn:
                _options.ExpiryMinutes * 60
        );
    }



    // This methon bt vakidate token and return UserId
    public string? ValidateToken(string token)
    {
        var tokenHandler =
            new JwtSecurityTokenHandler();


        var symmetricSecurityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _options.Key
                )
            );


        try
        {
            var principal =
                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        IssuerSigningKey =
                            symmetricSecurityKey,

                        ValidateIssuerSigningKey =
                            true,

                        ValidateIssuer =
                            true,

                        ValidIssuer =
                            _options.Issuer,

                        ValidateAudience =
                            true,

                        ValidAudience =
                            _options.Audience,

                        /*
                         * Important:
                         * Refresh flow may receive
                         * an expired access token.
                         */
                        ValidateLifetime =
                            false,

                        RoleClaimType =
                            ClaimTypes.Role,

                        NameClaimType =
                            ClaimTypes.NameIdentifier,

                        ClockSkew =
                            TimeSpan.Zero
                    },
                    out _
                );


            return principal
                .FindFirstValue(
                    ClaimTypes.NameIdentifier
                )
                ??
                principal
                    .FindFirstValue(
                        JwtRegisteredClaimNames.Sub
                    );
        }
        catch
        {
            return null;
        }
    }

}