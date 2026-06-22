using System.Text;
using System.Threading.RateLimiting;
using CommonGround.Api.Application;
using CommonGround.Api.Auth;
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
using Microsoft.AspNetCore.HttpOverrides;
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

// Shared submission pipeline (used by POST /api/responses and the invitee join)
builder.Services.AddScoped<ResponseSubmissionService>();

// Transactional invitee-join orchestration (US2)
builder.Services.AddScoped<InviteJoinService>();

// Controllers
builder.Services.AddControllers();

// Session JWT issuer (T032) — mints the cg_session cookie token
builder.Services.AddScoped<SessionTokenIssuer>();

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

// Forwarded headers — Render (and any reverse proxy) terminates TLS at its edge and
// forwards the original scheme/client IP in X-Forwarded-*. Honor them so the app sees
// the request as https (correct redirects, scheme-aware behavior, real client IPs).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // The platform's proxy is the only hop in front of us; trust its forwarded headers.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Rate limiting (T012) — POST endpoints. The permit limit is read lazily (per
// request, from DI config) rather than eagerly: WebApplicationFactory applies
// the integration-test config override AFTER Program.cs's inline reads run, so
// an eager read would miss it (same reason the JWT key is read inside AddJwtBearer).
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PostPolicy", httpContext =>
    {
        var permitLimit = int.TryParse(
            httpContext.RequestServices.GetRequiredService<IConfiguration>()["RateLimiting:PostPermitLimit"],
            out var limit) ? limit : 10;

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = permitLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            });
    });
});

var app = builder.Build();

// Apply EF migrations (schema + baked seed data) on startup when explicitly opted in.
// Off by default so the app never migrates a shared database unexpectedly; the
// containerized local stack (`docker compose --profile full up`) enables it via
// RunMigrationsOnStartup=true. For multi-instance deployments, prefer running
// migrations as a discrete release step instead of enabling this on every replica.
if (app.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

// Honor X-Forwarded-* from the platform proxy before anything reads the scheme/IP.
app.UseForwardedHeaders();

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

// Serve the built React SPA (wwwroot) from the same origin as the API, so the
// cg_session cookie (SameSite=Strict) is first-party. Controllers match API routes
// first; any other path falls back to index.html for client-side routing (e.g. /me).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

await app.RunAsync();
