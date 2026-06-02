using System.Text;
using CommonGround.Api.Persistence;
using CommonGround.Modules.Privacy.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace CommonGround.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestHmacKey = "test-hmac-key-minimum-32-characters-xyz";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithDatabase("commonground_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // DB connection string is read lazily in the AddDbContext lambda, so this works.
        // JWT SecretKey is now also read lazily inside AddJwtBearer, so it also works.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _db.GetConnectionString(),
                ["Jwt:SecretKey"] = "test-secret-key-minimum-32-characters-abc",
                ["Jwt:Issuer"] = "CommonGround.Test",
                ["Jwt:Audience"] = "CommonGround.Test",
                ["Jwt:ExpiryDays"] = "1",
                ["Privacy:HmacKey"] = TestHmacKey,
            });
        });

        // TokenService is registered eagerly in AddPrivacyModule, so we re-register
        // after startup with the test key. The last registration wins.
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(new TokenService(Encoding.UTF8.GetBytes(TestHmacKey)));
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _db.StartAsync();
        await MigrateDatabaseAsync();
    }

    async Task IAsyncLifetime.DisposeAsync() => await _db.DisposeAsync();

    private async Task MigrateDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
