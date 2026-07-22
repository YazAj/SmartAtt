using AttendAI.Application.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AttendAI.Infrastructure.Identity;

public sealed class IdentitySeedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentitySeedService> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentitySeedService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<IdentitySeedService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var roleName in RoleConstants.All)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to create role '{roleName}': {FormatErrors(roleResult)}");
                }
            }
        }

        var adminEmail = _configuration["DemoAdmin:Email"];
        var adminPassword = _configuration["DemoAdmin:Password"];
        var adminFullName = _configuration["DemoAdmin:FullName"] ?? "AttendAI Demo Administrator";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            _logger.LogInformation("Demo administrator was not seeded because DemoAdmin credentials were not configured.");
            return;
        }

        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            existingAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminFullName
            };

            var userResult = await _userManager.CreateAsync(existingAdmin, adminPassword);
            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to create demo administrator: {FormatErrors(userResult)}");
            }
        }

        if (!await _userManager.IsInRoleAsync(existingAdmin, RoleConstants.Admin))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(existingAdmin, RoleConstants.Admin);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to assign demo administrator role: {FormatErrors(addRoleResult)}");
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private static string FormatErrors(IdentityResult result)
        => string.Join("; ", result.Errors.Select(error => error.Description));
}
