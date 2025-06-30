using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring JWT tokens in the test projects.
/// </summary>
public static class UnsignedJwtServiceCollectionExtensions
{
    /// <summary>
    /// Configures the JWT handler to accept any token for testing purposes.
    /// </summary>
    public static IServiceCollection ConfigureUnsignedJwtBearerTokens(this IServiceCollection serviceCollection, string scheme = JwtBearerDefaults.AuthenticationScheme)
        => serviceCollection.PostConfigure<JwtBearerOptions>(scheme, o => {
            o.Authority = null;
            o.Audience = null;
            o.Configuration = null;
            o.ConfigurationManager = null;
            o.TokenValidationParameters = new TokenValidationParameters {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidateActor = false,
                ValidateIssuerSigningKey = false,
                ValidateTokenReplay = false,
                ValidateWithLKG = false,
                RequireSignedTokens = false,
            };
        });
}
