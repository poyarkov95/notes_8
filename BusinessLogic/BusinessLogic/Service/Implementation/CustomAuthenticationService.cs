using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BusinessLogic.Service.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Service.Implementation;

public class CustomAuthenticationService(IHttpContextAccessor httpContext)
    : ICustomAuthenticationService
{
    public async Task<AuthenticateResult> Authenticate()
    {
        var endpoint = httpContext.HttpContext?.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            return AuthenticateResult.NoResult();
        }
        
        var base64Token = httpContext.HttpContext?.Request.Headers["access_token"];
        var jwtToken = new JwtSecurityTokenHandler().ReadToken(base64Token) as JwtSecurityToken;

        if (jwtToken == null)
        {
            return AuthenticateResult.Fail(new Exception("Invalid token"));
        }

        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            return AuthenticateResult.Fail(new Exception("Token expired"));
        }
            
        var claims = jwtToken.Claims.ToList();
            
        var identity = new ClaimsIdentity(claims, "BasicSchema");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "BasicSchema");
        return AuthenticateResult.Success(ticket);
    }
}