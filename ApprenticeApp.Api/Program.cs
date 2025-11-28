using ApprenticeApp.Core.Data;
using ApprenticeApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var envConnection = Environment.GetEnvironmentVariable("APPRENTICEAPP_CONNECTION");
if (!string.IsNullOrWhiteSpace(envConnection)) // override if given
{
    connectionString = envConnection;
}

connectionString ??= "Data Source=apprentice.db";

builder.Services.AddDbContext<ApprenticeDbContext>(options =>
{
    options.UseSqlite(connectionString, builder => builder.MigrationsAssembly("ApprenticeApp.Api"));
});

builder.Services.AddScoped<IApprenticeRepository, ApprenticeRepository>(); // DI
builder.Services.AddScoped<IMentorRepository, MentorRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApprenticeDbContext>();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // middleware
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/error"); // for Prod
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ApprenticeApp.Api.Hubs.ApprenticeHub>("/hubs/apprentices");

app.Map("/error", () => Results.Problem("An unexpected error occurred."));

app.Run();
