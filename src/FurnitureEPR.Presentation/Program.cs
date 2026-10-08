using FurnitureEPR.Application.Security;
using FurnitureEPR.Infrastructure;
using FurnitureEPR.Infrastructure.Identity;
using FurnitureEPR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
    throw new InvalidOperationException("Jwt:Key is required.");

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.Configure<IdentitySeedOptions>(
    builder.Configuration.GetSection(IdentitySeedOptions.SectionName));
builder.Services.AddScoped<IdentitySeeder>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PermissionNames.WorkflowMove, policy =>
        policy.RequireClaim(IdentityClaimTypes.Permission, PermissionNames.WorkflowMove));

    options.AddPolicy(PermissionNames.WorkflowQualityControl, policy =>
        policy.RequireClaim(
            IdentityClaimTypes.Permission,
            PermissionNames.WorkflowQualityControl));

    options.AddPolicy(PermissionNames.WorkflowComplete, policy =>
        policy.RequireClaim(
            IdentityClaimTypes.Permission,
            PermissionNames.WorkflowComplete));
});

builder.Services
    .AddIdentityCore<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await identitySeeder.SeedAsync();
}

app.Run();
