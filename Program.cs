using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Microsoft.Graph;
using EventManagementApi.Entities;
using Azure.Identity;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration.GetSection("EntraId"));

// Configure Microsoft Graph SDK using Azure Identity
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var tenantId = configuration["EntraId:TenantId"];
    var clientId = configuration["EntraId:ClientId"];
    var clientSecret = configuration["EntraId:ClientSecret"];
    var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
    return new GraphServiceClient(clientSecretCredential);
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Management System API", Version = "v1" });

    // Add security requirements for Swagger
    var scopes = new Dictionary<string, string>
    {
       { "User.Read", "Read user information" },
        { "User.ReadWrite.All", "Read and write user information" }
    };
    c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
    {
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference {
                Type = ReferenceType.SecurityScheme,
                Id = "oauth2"
            },
            Scheme = "oauth2",
            Name = "oauth2",
            In = ParameterLocation.Header
        },
        new List <string> ()
    }
});

    // Add security definitions for Swagger
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/dc6635dc-1fdd-498d-a9d9-3a8b5c1d0eca/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://login.microsoftonline.com/dc6635dc-1fdd-498d-a9d9-3a8b5c1d0eca/oauth2/v2.0/token"),
                Scopes = scopes
            }
        }
    });

    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

// Add Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.OAuthAppName("Ezure Management API Secret");
    options.OAuthClientId("b8d05ced-4859-4949-b21a-1070822d99f4");
    options.OAuthClientSecret("D-b8Q~eDCnRkKj6UTylCZMLPvjv5nun6hWllLcyT");
    options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
});

// Middleware configurations
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();