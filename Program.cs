using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RealEstate.Data;
using RealEstate.Interfaces;
using RealEstate.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel Server limits to allow for large multiple image uploads
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104_857_600; // 100 MB limit
});

// Helper to copy premium assets from brain directory at startup
try
{
    var sourceDir = @"C:\Users\Shintoe\.gemini\antigravity\brain\dafabc1a-da66-42d8-a9d3-747a5613d41f";
    var destDir = Path.Combine(Directory.GetCurrentDirectory(), "frontend", "public", "assets");
    if (!Directory.Exists(destDir))
    {
        Directory.CreateDirectory(destDir);
    }
    
    var villaSource = Path.Combine(sourceDir, "villa_sunset_1779187122092.png");
    if (File.Exists(villaSource))
    {
        File.Copy(villaSource, Path.Combine(destDir, "villa_sunset.png"), true);
    }
    
    var logoSource = Path.Combine(sourceDir, "tbee_estates_logo_1779651099569.png");
    if (File.Exists(logoSource))
    {
        File.Copy(logoSource, Path.Combine(destDir, "logo.png"), true);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error copying assets: {ex.Message}");
}

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references in JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Real Estate Management API", Version = "v1" });
});

// Configure Database Connection: Fallback to InMemory Database if connection string isn't found or for ease of local run
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Extremely developer-friendly fallback to InMemory for direct standalone run
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("RealEstateDb"));
}

// Register all repositories
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<ILeaseRepository, LeaseRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowAngular");

// Automatically seed some initial data into the database at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Recreate DB with latest schema in Development for instant prototyping
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("==================================================================");
        Console.WriteLine("❌ DATABASE INITIALIZATION ERROR:");
        Console.WriteLine(ex.Message);
        Console.WriteLine("👉 Active connection locks detected! Please close all other running dotnet instances.");
        Console.WriteLine("👉 Run this command in your PowerShell: Stop-Process -Name dotnet -Force");
        Console.WriteLine("==================================================================");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Real Estate Management API V1");
        c.RoutePrefix = string.Empty; // Swagger UI as root
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();
