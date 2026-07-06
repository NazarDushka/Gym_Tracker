using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("UserId", "1"),
                new Claim("Id", "1")
            };

            if (Context.Request.Headers.TryGetValue("X-Test-Role", out var roleValue))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleValue.ToString()));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}