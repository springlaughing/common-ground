using System.Text;
using System.Threading.RateLimiting;
using CommonGround.Api.Persistence;
using CommonGround.Modules.Audit;
using CommonGround.Modules.Audit.Services;
using CommonGround.Modules.Comparisons;
using CommonGround.Modules.Privacy;
using CommonGround.Modules.Questionnaires;
using CommonGround.Modules.Reporting;
using CommonGround.Modules.Responses;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Expose AppDbContext as DbContext for modules that depend on it
builder.Services.AddScoped<Microsoft.EntityFrameworkCore.DbContext>(
    sp => sp.GetRequiredService<AppDbContext>());

// Modules
var hmacKey = Encoding.UTF8.GetBytes(
    builder.Configuration["Privacy:HmacKey"]
    ?? throw new InvalidOperationException("Privacy:HmacKey is required"));

builder.Services
    .AddQuestionnairesModule()
    .AddResponsesModule()
    .AddReportingModule()
    .AddPrivacyModule(hmacKey)
    .AddAuditModule()
    .AddComparisonsModule();

// Audit logger
builder.Services.AddScoped<IAuditLogger, EfAuditLogger>();

// Controllers
builder.Services.AddControllers();

// JWT authentication (T011) — key is read inside the lambda so test overrides apply
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is required"));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
        options.Events = new JwtBearerEvents
        {
            // Read JWT from HttpOnly cookie instead of Authorization header
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["cg_session"];
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

// CORS (T012)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Rate limiting (T012) — POST endpoints
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PostPolicy", _ =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));
});

var app = builder.Build();

// Security headers (T012)
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.XContentTypeOptions = "nosniff";
    ctx.Response.Headers.XFrameOptions = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers.XXSSProtection = "0";
    await next();
});

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
