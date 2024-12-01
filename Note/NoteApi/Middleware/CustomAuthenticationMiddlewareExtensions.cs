using Microsoft.AspNetCore.Authentication;

namespace NoteApi.Middleware;

public static class CustomAuthenticationMiddlewareExtensions
{
    public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<AuthenticationSchemeOptions> configureOptions)
    {
        return builder.AddScheme<AuthenticationSchemeOptions, AuthenticationMiddleware>(authenticationScheme, displayName, configureOptions);
    }
}