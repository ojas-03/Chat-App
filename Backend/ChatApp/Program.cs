using ChatApp.Data;
using ChatApp.Hubs;
using ChatApp.Services;
using ChatApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Disable Windows EventLog logging provider to prevent access-denied logger crashes
if (OperatingSystem.IsWindows())
{
    builder.Logging.AddFilter<Microsoft.Extensions.Logging.EventLog.EventLogLoggerProvider>(null, LogLevel.None);
}

// Configure FormOptions to buffer multipart uploads up to 50MB in RAM
// Prevents ASP.NET Core from writing to C:\Users\...\AppData\Local\Temp (which triggers Access Denied)
builder.Services.Configure<FormOptions>(options =>
{
    options.MemoryBufferThreshold = 50 * 1024 * 1024; // 50 MB
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

// ----- Database -----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----- Authentication (Hybrid Cookie + Bearer + SignalR Query String) -----
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
        {
            secretKey = "DefaultDevelopmentSuperSecretKeyForChatApp1234567890!";
        }
        var key = Encoding.UTF8.GetBytes(secretKey);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // 1. SignalR WebSocket handshake — token sent via query string
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                // 2. Cookie fallback — web clients send JWT in httpOnly cookie
                else if (context.Request.Cookies.TryGetValue("jwt", out var cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// ----- Application Services -----
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IKeyService, KeyService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddSingleton<IPresenceTracker, InMemoryPresenceTracker>();

// ----- SignalR -----
builder.Services.AddSignalR();

// ----- Controllers -----
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// ----- CORS -----
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
