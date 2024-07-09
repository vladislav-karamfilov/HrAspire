using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<EmployeesDbContext>("employees-db");

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies(options => options.ApplicationCookie!
        .Configure(cookieOptions =>
        {
            cookieOptions.Cookie.SameSite = SameSiteMode.None;
            cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        }));

builder.Services.AddAuthorizationBuilder();

builder.Services
    .AddIdentityCore<Employee>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EmployeesDbContext>()
    .AddApiEndpoints();

var app = builder.Build();

// TODO: Extract to another service - console app for seeding data if on dev env
using (var scope = app.Services.CreateScope())
{
    using var db = scope.ServiceProvider.GetRequiredService<EmployeesDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(policyBuilder => policyBuilder
    .WithOrigins(app.Configuration["WebFrontEndUrl"]!)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.MapIdentityApi<Employee>();

app.UseAuthentication();
app.UseAuthorization();

// https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity#antiforgery-support
app
    .MapPost("/logout", async (SignInManager<Employee> signInManager, [FromBody] object empty) =>
    {
        if (empty is not null)
        {
            await signInManager.SignOutAsync();

            return Results.Ok();
        }

        return Results.Unauthorized();
    })
    .RequireAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapDefaultEndpoints();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
