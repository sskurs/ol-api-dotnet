using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using ol_api_dotnet.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

// Register services
builder.Services.AddScoped<CustomEventService>();
builder.Services.AddSingleton<EarningRuleEngineService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS before authentication/authorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
Console.WriteLine("🛣️ Controllers mapped");

// Debug: List all registered routes
var endpoints = app.Services.GetRequiredService<IEnumerable<EndpointDataSource>>();
foreach (var endpoint in endpoints.SelectMany(es => es.Endpoints))
{
    if (endpoint is RouteEndpoint routeEndpoint)
    {
        Console.WriteLine($"🔍 Route: {routeEndpoint.RoutePattern.RawText}, Method: {string.Join(", ", routeEndpoint.Metadata.OfType<HttpMethodMetadata>().SelectMany(m => m.HttpMethods))}");
    }
}

SeedDatabase(app);

app.Run();

void SeedDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Users.Any())
    {
        var users = new List<User>
        {
            new User { FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", PasswordHash = "hash1", Role = "user" },
            new User { FirstName = "Bob", LastName = "Jones", Email = "bob@example.com", PasswordHash = "hash2", Role = "user" },
            new User { FirstName = "Carol", LastName = "Lee", Email = "carol@example.com", PasswordHash = "hash3", Role = "user" }
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        var alice = db.Users.First(u => u.Email == "alice@example.com");
        var bob = db.Users.First(u => u.Email == "bob@example.com");
        var carol = db.Users.First(u => u.Email == "carol@example.com");

        db.Transactions.AddRange(new[]
        {
            new Transaction { UserId = alice.Id, Amount = 100, Type = "purchase", Date = DateTime.UtcNow.AddDays(-2) },
            new Transaction { UserId = alice.Id, Amount = 50, Type = "reward", Date = DateTime.UtcNow.AddDays(-1) },
            new Transaction { UserId = bob.Id, Amount = 200, Type = "purchase", Date = DateTime.UtcNow.AddDays(-3) },
            new Transaction { UserId = carol.Id, Amount = 150, Type = "purchase", Date = DateTime.UtcNow.AddDays(-5) }
        });

        db.Points.AddRange(new[]
        {
            new Points { UserId = alice.Id, Balance = 500 },
            new Points { UserId = bob.Id, Balance = 300 },
            new Points { UserId = carol.Id, Balance = 800 }
        });

        db.Tiers.AddRange(new[]
        {
            new Tier { UserId = alice.Id, Level = "gold", AssignedAt = DateTime.UtcNow.AddMonths(-2) },
            new Tier { UserId = bob.Id, Level = "silver", AssignedAt = DateTime.UtcNow.AddMonths(-1) },
            new Tier { UserId = carol.Id, Level = "bronze", AssignedAt = DateTime.UtcNow.AddMonths(-3) }
        });

        db.SaveChanges();

        // Seed initial earning rule if none exists
        if (!db.EarningRules.Any())
        {
            var ruleJson = @"
            {
                ""WorkflowName"": ""PurchaseRule"",
                ""Rules"": [
                    {
                        ""RuleName"": ""PurchaseAmountGreaterThan5000"",
                        ""ErrorMessage"": ""Purchase amount is not greater than 5000."",
                        ""ErrorType"": ""Error"",
                        ""RuleExpressionType"": ""LambdaExpression"",
                        ""Expression"": ""input.PurchaseAmount > 5000"",
                        ""Actions"": {
                            ""OnSuccess"": {
                                ""Name"": ""Evaluate"",
                                ""Context"": {
                                    ""Expression"": ""100""
                                }
                            }
                        }
                    }
                ]
            }";

            db.EarningRules.Add(new EarningRule
            {
                Name = "Purchase Amount Greater Than 5000",
                RuleJson = ruleJson,
                IsActive = true
            });

            db.SaveChanges();
        }
    }
}
