using Azure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ordering.API.Features.Auth.Login;
using Ordering.API.Features.Auth.Register;
using Ordering.API.Features.Orders.CreateOrder;
using Ordering.API.Infrastructure.Auth;
using Ordering.API.Infrastructure.Data;
using Ordering.API.Infrastructure.Middleware;
using Scalar.AspNetCore;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using MassTransit;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = builder.Configuration["AzureKeyVault:Endpoint"];
var serviceName = "Ordering.API";

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
builder.Services.AddMassTransit(x =>
{
	x.AddEntityFrameworkOutbox<OrderingContext>(o =>
	{
		o.UseSqlServer();
		o.UseBusOutbox();
	});

	x.SetKebabCaseEndpointNameFormatter();

	x.UsingAzureServiceBus((context, cfg) =>
	{
		cfg.Host(builder.Configuration["ServiceBus:ConnectionString"]);
		cfg.UseMessageRetry(r => r.Exponential(
			retryLimit: 4,
			minInterval: TimeSpan.FromSeconds(1),
			maxInterval: TimeSpan.FromSeconds(30),
			intervalDelta: TimeSpan.FromSeconds(2)
		));
		cfg.ConfigureEndpoints(context);
	});
});
builder.Services.AddOpenTelemetry().WithTracing(tracing =>
{
    tracing.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddEntityFrameworkCoreInstrumentation()
    .AddSqlClientInstrumentation()
    .AddSource("MassTransit")
    .AddOtlpExporter(op =>
        op.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317")
    );
});
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

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
		options.Events = new JwtBearerEvents
		{
			OnAuthenticationFailed = context =>
			{
				Console.WriteLine("--- JWT AUTH FAILED ---");
				Console.WriteLine($"Error: {context.Exception.Message}");
				return Task.CompletedTask;
			}
		};
	});

builder.Services.AddAuthorization();
builder.Host.UseSerilog((context, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console();
});
builder.Services.AddScoped<CreateOrderHandler>();
var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	using (var scope = app.Services.CreateScope())
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<OrderingContext>();
		await dbContext.Database.MigrateAsync();
		await DbSeeder.SeedAsync(dbContext);
	}
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
app.MapCreateOrder();
app.Run();
