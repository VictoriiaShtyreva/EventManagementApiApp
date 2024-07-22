using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Microsoft.Graph;
using EventManagementApi.Entities;
using Azure.Identity;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EventManagementApi.Services;
using Microsoft.Azure.Cosmos;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddTransient<RoleService>();

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

// Configure Cosmos DB for PostgreSQL Cluster connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Cosmos DB NoSQL connection
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var account = configuration["CosmosDb:Account"];
    var key = configuration["CosmosDb:Key"];
    return new CosmosClient(account, key);
});

// Configure authentication using Azure 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraID"));

// Configure authorisation policy
builder.Services.AddAuthorizationBuilder()
                                     // Configure authorisation policy
                                     .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
                                     // Configure authorisation policy
                                     .AddPolicy("UserOnly", policy => policy.RequireRole("User"))
                                     // Configure authorisation policy
                                     .AddPolicy("EventProviderOnly", policy => policy.RequireRole("EventProvider"));

//Configure Microsoft Graph SDK using Azure Identity
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var tenantId = configuration["EntraId:TenantId"];
    var clientId = configuration["EntraId:ClientId"];
    var clientSecret = configuration["EntraId:ClientSecret"];
    var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
    return new GraphServiceClient(clientSecretCredential);
});

// Configure Azure Blob Storage
// builder.Services.AddSingleton(s => new BlobServiceClient(builder.Configuration["BlobStorage:ConnectionString"]));

//Configure Azure Service Bus
builder.Services.AddSingleton(s => new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

// Configure Azure Application Insights for monitoring and diagnostics
// builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);


//Add authorization for Swagger
builder.Services.AddSwaggerGen(
    options =>
    {
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Description = "Bearer token authentication",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Scheme = "Bearer"
        }
        );

        options.OperationFilter<SecurityRequirementsOperationFilter>();
    }
);


var app = builder.Build();

// Add Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
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