using Azure.Identity;
using IS7024FinalProject.Pages.API;
using IS7024FinalProject.Services;
using Microsoft.OpenApi;

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register IAPIService implementation with DI using IHttpClientFactory
builder.Services.AddHttpClient<IAPIService, APIService>();

builder.Services.AddSwaggerGen(o =>
{
    o.EnableAnnotations();
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EventParking",
        Version = "v1",
        Description = "API for searching events and parking options.",
    });

    o.UseAllOfToExtendReferenceSchemas();
    o.CustomSchemaIds(type => type.FullName);
});
builder.Services.AddHttpClient();

try
{
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // Show detailed errors locally
        app.UseDeveloperExceptionPage();

        // Swagger only active in Development until packages are verified
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventParking v1");
            // c.RoutePrefix = string.Empty; // uncomment to serve UI at app root
        });
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();
    app.MapStaticAssets();
    app.MapRazorPages().WithStaticAssets();

    // Register the GetEventSearch minimal endpoint
    GetEventSearch.Map(app);

    // Register the GetParkingSearch minimal endpoint
    GetParkingSearch.Map(app);

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