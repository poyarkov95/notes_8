using System.Text.Encodings.Web;
using BusinessLogic.Service.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace NoteApi.Middleware;

public class AuthenticationMiddleware(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ICustomAuthenticationService authenticationService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly ICustomAuthenticationService _authenticationService = authenticationService;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return _authenticationService.Authenticate();
    }
}