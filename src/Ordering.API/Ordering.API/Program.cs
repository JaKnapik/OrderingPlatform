using Azure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ordering.API.Features.Auth.Login;
using Ordering.API.Features.Auth.Register;
using Ordering.API.Infrastructure.Auth;
using Ordering.API.Infrastructure.Data;
using Ordering.API.Infrastructure.Middleware;
using Scalar.AspNetCore;
using Serilog;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = builder.Configuration["AzureKeyVault:Endpoint"];

if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}
else
{
    throw new Exception("No azure configuration");
}
var connectionString = builder.Configuration["DbConnectionString"];
var jwtKey = builder.Configuration["Jwt:Key"];
builder.Services.AddDbContext<OrderingContext>(
    options => options.UseSqlServer(connectionString));
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 8;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
}).AddEntityFrameworkStores<OrderingContext>()
.AddDefaultTokenProviders();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Host.UseSerilog((context, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console();
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Version = "1.0.0"
}))
.WithName("GetHealth")
.AddOpenApiOperationTransformer((op, context, ct) =>
{
    op.Summary = "Healthcheck endpoint";
    op.Description = "Returns healthcheck status";
    return Task.CompletedTask;
});

app.UseAuthentication();
app.UseAuthorization();
app.MapLogin();
app.MapRegister();
app.Run();
