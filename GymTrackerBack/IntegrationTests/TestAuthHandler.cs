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
        new Claim(ClaimTypes.Role, "User") // Default role
    };

            bool isIdFound = false;

            // Look for the standard HTTP Authorization header
            if (Context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString(); // Example: "TestScheme 2"

                // Check if this is our scheme and extract what comes after the space
                if (token.StartsWith("TestScheme ", StringComparison.OrdinalIgnoreCase))
                {
                    var userId = token.Substring("TestScheme ".Length).Trim();

                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                    claims.Add(new Claim("UserId", userId));
                    claims.Add(new Claim("Id", userId));
                    isIdFound = true;
                }
                if (token.StartsWith("TestSchemeAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    var userId = token.Substring("TestSchemeAdmin".Length).Trim();

                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                    claims.Add(new Claim("UserId", userId));
                    claims.Add(new Claim("Id", userId));
                    claims.Add(new Claim(ClaimTypes.Role, "Admin")); // Administrator role
                    isIdFound = true;
                }
            }

            // Fallback: if the header was without an ID (just "TestScheme"), set to 1
            if (!isIdFound)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "1"));
                claims.Add(new Claim("UserId", "1"));
                claims.Add(new Claim("Id", "1"));
            }

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}