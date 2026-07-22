using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AttendAI.Infrastructure.Identity;

namespace AttendAI.Infrastructure.Persistence;

public sealed class ApplicationDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IdentitySeedService _identitySeedService;
    private readonly ILogger<ApplicationDbInitializer> _logger;

    public ApplicationDbInitializer(
        ApplicationDbContext dbContext,
        IdentitySeedService identitySeedService,
        ILogger<ApplicationDbInitializer> logger)
    {
        _dbContext = dbContext;
        _identitySeedService = identitySeedService;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Applying AttendAI database migrations.");
        await _dbContext.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("Seeding AttendAI identity roles and optional demo administrator.");
        await _identitySeedService.SeedAsync(cancellationToken);
    }
}
