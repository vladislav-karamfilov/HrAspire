using HrAspire.Business.Common;
using HrAspire.Employees.Data;
using HrAspire.Employees.Data.Models;
using HrAspire.Employees.Web;
using HrAspire.Web.ApiGateway;
using HrAspire.Web.ApiGateway.Endpoints;
using HrAspire.Web.Common;

using Microsoft.AspNetCore.Identity;

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

builder.Services.AddGrpcClient<Employees.EmployeesClient>(o => o.Address = new Uri("https://employees-service"));
builder.Services.AddGrpcClient<Documents.DocumentsClient>(o => o.Address = new Uri("https://employees-service"));

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(Constants.ManagerAuthPolicyName, p => p.RequireRole(BusinessConstants.ManagerRole))
    .AddPolicy(Constants.HrManagerAuthPolicyName, p => p.RequireRole(BusinessConstants.HrManagerRole))
    .AddPolicy(
        Constants.ManagerOrHrManagerAuthPolicyName,
        p => p.RequireRole(BusinessConstants.ManagerRole, BusinessConstants.HrManagerRole));

builder.Services
    .AddIdentityCore<Employee>(options => options.Password.RequiredLength = AccountConstants.PasswordMinLength)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EmployeesDbContext>()
    .AddApiEndpoints();

var app = builder.Build();

// TODO: Extract to another service - console app for seeding data if on dev env
using (var scope = app.Services.CreateScope())
{
    using var db = scope.ServiceProvider.GetRequiredService<EmployeesDbContext>();
    await db.Database.EnsureCreatedAsync();

    if (!db.Roles.Any())
    {
        db.Roles.Add(new IdentityRole { Id = BusinessConstants.ManagerRole, Name = BusinessConstants.ManagerRole });
        db.Roles.Add(new IdentityRole { Id = BusinessConstants.HrManagerRole, Name = BusinessConstants.HrManagerRole });

        await db.SaveChangesAsync();
    }
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

app.UseAuthentication();
app.UseAuthorization();

app.MapAccountEndpoints();
app.MapEmployeesEndpoints();
app.MapDocumentsEndpoints();

app.MapDefaultEndpoints();

app.Run();
