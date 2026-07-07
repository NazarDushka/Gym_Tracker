using GymTracker.Repository;
using IntegrationTests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GymTracker.IntegrationTests
{
    public class GymTrackerWebApplicationFactory : WebApplicationFactory<Program>
    {
        // 1. Create a unique physical root for this factory
        private readonly InMemoryDatabaseRoot _dbRoot = new InMemoryDatabaseRoot();

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

                services.AddDbContext<WorkoutDbContext>(options =>
                {
                    // 2. Pass _dbRoot here. The database name can now be anything (even static), 
                    // because _dbRoot guarantees absolute isolation in memory.
                    options.UseInMemoryDatabase("IntegrationTestDb", _dbRoot);
                });

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                })
                   .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }
    }
}