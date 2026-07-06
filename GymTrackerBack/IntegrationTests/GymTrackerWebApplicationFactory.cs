using GymTracker.Repository;
using IntegrationTests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GymTracker.IntegrationTests
{
    public class GymTrackerWebApplicationFactory : WebApplicationFactory<Program>
    {
        public GymTrackerWebApplicationFactory()
        {
            Program.IsTestRun = true;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<WorkoutDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

                string inMemoryDbName = $"TestDb_{Guid.NewGuid()}";
            
                services.AddDbContext<WorkoutDbContext>(options =>
                {
                    options.UseInMemoryDatabase(inMemoryDbName);
                });
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                }
            });
        }
    }
}