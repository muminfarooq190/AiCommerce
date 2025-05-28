using Ecommerce.Configurations;
using Ecommerce.Utilities;
using EcommerceApi;
using EcommerceApi.Entities.DbContexts;
using EcommerceApi.Middlewares;
using EcommerceApi.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDependencies();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient(serviceProvider =>
{
    var tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    string? companyName = tenantProvider.GetCurrentTenant()?.CompanyName;
    if (string.IsNullOrWhiteSpace(companyName))
    {
        companyName = configuration["TenantDabaseSchema"] ?? throw new ArgumentNullException("TenantDabaseSchema is not configured properly in appsettings");
    }

    return new SchemaProvider(companyName);
});

builder.Services.AddScoped(serviceProvider =>
{
    var options = serviceProvider
        .GetRequiredService<DbContextOptions<TenantDbContext>>();
    return new TenantDbContext(options, serviceProvider);
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped(serviceProvider =>
{
    var options = serviceProvider
        .GetRequiredService<DbContextOptions<AppDbContext>>();

    return new AppDbContext(options, serviceProvider);
});


builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddScoped<JwtTokenGenerator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Optional:
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ecommerce API",
        Version = "v1",
        Description = "API for managing permissions and tenants"
    });
});

JwtSettings jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? throw new ArgumentNullException("JwtSettings is not configured");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<TenentExtracterMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();