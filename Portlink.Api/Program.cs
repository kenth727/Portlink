using System.Text;
using PortlinkApp.Api.Configuration;
using PortlinkApp.Api.Middleware;
using PortlinkApp.Api.Services;
using PortlinkApp.Core.Data;
using PortlinkApp.Core.Entities;
using PortlinkApp.Core.Repositories;
using PortlinkApp.Core.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/portlink-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var connectionString = Environment.GetEnvironmentVariable("PORTLINK_CONNECTION")
    ?? Environment.GetEnvironmentVariable("APPRENTICEAPP_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=portlink;Username=portadmin;Password=Port@Dev2024";

builder.Services.AddDbContext<PortlinkDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings for demo (relaxed for ease of use)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<PortlinkDbContext>()
.AddDefaultTokenProviders();

// Configure JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var key = Encoding.UTF8.GetBytes(jwtSettings.Get<JwtSettings>()!.Secret);

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
        ValidIssuer = jwtSettings.Get<JwtSettings>()!.Issuer,
        ValidAudience = jwtSettings.Get<JwtSettings>()!.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PortOperator", policy => policy.RequireRole("PortOperator"));
    options.AddPolicy("Viewer", policy => policy.RequireRole("Viewer", "PortOperator"));
});

builder.Services.AddHttpClient<IAIService, LMStudioAIService>();
builder.Services.AddSingleton<LoadSimulatorService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<LoadSimulatorService>());

builder.Services.AddScoped<IVesselRepository, VesselRepository>();
builder.Services.AddScoped<IBerthRepository, BerthRepository>();
builder.Services.AddScoped<IPortCallRepository, PortCallRepository>();

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddCheck<LmStudioHealthCheck>("lm-studio");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PortlinkDbContext>();
    await db.Database.MigrateAsync();

    // Seed roles and users
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await AuthDbInitializer.SeedAsync(userManager, roleManager);

    // Seed maritime data
    await MaritimeDbInitializer.SeedAsync(db);
}

// Use global exception handler middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// NOTE: For this sample we keep HTTP only for local development / dashboard use.
// If you later deploy behind a real TLS terminator or reverse proxy,
// re‑enable HTTPS redirection there instead of here.
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");

// IMPORTANT: Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<PortlinkApp.Api.Hubs.PortOperationsHub>("/hubs/port-operations");

app.Map("/error", () => Results.Problem("An unexpected error occurred."));

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
