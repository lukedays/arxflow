using Microsoft.EntityFrameworkCore;
using Serilog;
using ArxFlow.Server.Data;
using ArxFlow.Server.Services;
using ArxFlow.Server.Endpoints;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Iniciando ArxFlow API");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Database - SQLite
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "arxflow.db");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));

    // HttpClients para serviços de download
    builder.Services.AddHttpClient<B3DownloadService>();
    builder.Services.AddHttpClient<B3InstrumentsService>();
    builder.Services.AddHttpClient<B3RendaFixaService>();
    builder.Services.AddHttpClient<BcbDownloadService>();
    builder.Services.AddHttpClient<AnbimaDownloadService>();
    builder.Services.AddHttpClient<AnbimaVNADownloadService>();

    // Services - Scoped
    builder.Services.AddScoped<BoletaService>();
    builder.Services.AddScoped<AtivoService>();
    builder.Services.AddScoped<EmissorService>();
    builder.Services.AddScoped<FundoService>();
    builder.Services.AddScoped<ContraparteService>();

    // CORS - permitir frontend Vite
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5236")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // OpenAPI nativo do .NET 9
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Middleware pipeline
    app.UseCors("AllowFrontend");

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Seed do banco de dados
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Criar diretório Data se não existir
            var dataDir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
                Log.Information("Created Data directory: {DataDir}", dataDir);
            }

            await db.Database.MigrateAsync();
            await DatabaseSeeder.SeedAsync(db);
            Log.Information("Database migrated and seeded successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during database migration or seeding");
            throw;
        }
    }

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
       .WithTags("Health")
       .WithOpenApi();

    // Mapear endpoints Minimal APIs
    app.MapEmissoresEndpoints();
    app.MapFundosEndpoints();
    app.MapContrapartesEndpoints();
    app.MapAtivosEndpoints();
    app.MapCalculadoraEndpoints();
    app.MapBoletasEndpoints();
    app.MapYieldCurveEndpoints();
    app.MapDownloadsEndpoints();

    // Servir frontend React
    app.UseDefaultFiles();
    app.MapStaticAssets();
    app.MapFallbackToFile("/index.html");

    Log.Information("ArxFlow API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
