using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Azure.Storage.Blobs;
using Azure.Messaging.ServiceBus;
using Microsoft.OpenApi.Models;
using Microsoft.Graph;
using EventManagementApi.Entities;
using Azure.Identity;
using Azure.Core;
using EventManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSingleton<RoleService>();
// Seed Data
builder.Services.AddScoped<SeedData>();

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure authentication to use Azure AD Entra ID with JWT Bearer tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraId"));

// Configure Microsoft Graph SDK using Azure Identity
var clientSecretCredential = new ClientSecretCredential(
    builder.Configuration["EntraId:TenantId"],
    builder.Configuration["EntraId:ClientId"],
    builder.Configuration["EntraId:ClientSecret"]);

builder.Services.AddSingleton<TokenCredential>(clientSecretCredential);
builder.Services.AddSingleton<GraphServiceClient>(provider =>
{
    var credential = provider.GetRequiredService<TokenCredential>();
    return new GraphServiceClient(credential);
});

// Configure role-based authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EventProvider", policy => policy.RequireRole("EventProvider"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

// Configure Azure Blob Storage
// builder.Services.AddSingleton(s => new BlobServiceClient(builder.Configuration["BlobStorage:ConnectionString"]));

// Configure Azure Service Bus
// builder.Services.AddSingleton(s => new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

// Configure Azure Application Insights for monitoring and diagnostics
// builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);

// Configure Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Event Management System API",
        Version = "v1"
    });

    // Add security definitions for Swagger
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Bearer token authentication",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    }
         );
});

var app = builder.Build();

// Add Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Management System API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

// Middleware configurations
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = services.GetRequiredService<SeedData>();
    await seeder.SeedDataAsync();
}

app.Run();