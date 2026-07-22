using AttendAI.Infrastructure.Persistence;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AttendAI.IntegrationTests;

public sealed class DatabaseConfigurationTests : IClassFixture<AttendAiWebApplicationFactory>
{
    private readonly AttendAiWebApplicationFactory _factory;

    public DatabaseConfigurationTests(AttendAiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Test_environment_uses_sqlite_database_provider()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", dbContext.Database.ProviderName);
    }
}
