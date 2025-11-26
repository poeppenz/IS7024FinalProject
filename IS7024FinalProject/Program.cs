using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

// 1. Initial Configuration Setup (Before builder.Build())
var builder = WebApplication.CreateBuilder(args);
var keyVaultUrl = builder.Configuration["KeyVault:VaultUri"];

// Use a simple, non-DI logger for this early phase
var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();

if (!string.IsNullOrEmpty(keyVaultUrl))
{
    try
    {
        logger.LogInformation("Attempting to connect to Key Vault at: {KeyVaultUrl}", keyVaultUrl);
        
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential()
        );
        logger.LogInformation("Successfully configured Azure Key Vault.");
    }
    catch (Exception ex)
    {
        // Log the fatal error and re-throw, as configuration is mandatory.
        logger.LogCritical(ex, "FATAL ERROR: Failed to load configuration from Azure Key Vault.");
        throw; // The app must exit here if it relies on Key Vault secrets.
    }
}
else
{
    logger.LogWarning("KeyVault:VaultUri is empty. Skipping Key Vault configuration.");
}

// Add the rest of your services
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();


// 2. Application Pipeline Build and Run Phase
try
{
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // **CAUTION**: UseMigrationsEndPoint is a common crash point for non-DB apps.
        // It's still present here, but wrapped in the main try/catch.
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();
    app.MapStaticAssets();
    app.MapRazorPages().WithStaticAssets();

    app.Run();
}
catch (Exception ex)
{
    // This catches exceptions from builder.Build() or anything in the pipeline setup.
    logger.LogCritical(ex, "FATAL ERROR: Application host terminated unexpectedly during build or run.");
    
    // Ensure the process exits with a non-zero code to signal failure
    Environment.ExitCode = 1;
}

// Dummy class to allow logging before the host is fully built
internal partial class Program { }