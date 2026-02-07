using JwtCleanArch.Application.Common;
using JwtCleanArch.Application.Interfaces;
using JwtCleanArch.Infrastructure.Data;
using JwtCleanArch.Infrastructure.Services;
using JwtCleanArch.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Ensure jwtSettings is not null before using its properties
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings section is missing or invalid.");

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),

    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            string authorization = context.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorization))
            {
                Console.WriteLine("[DEBUG] No Authorization header found.");
                return Task.CompletedTask;
            }

            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // Extract just the token part
                var token = authorization.Substring("Bearer ".Length).Trim();
                Console.WriteLine($"[DEBUG] Cleaned Token: {token}");

                // This ensures the middleware uses the cleaned string
                context.Token = token;
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[ERROR] Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("[SUCCESS] Token validated successfully.");
            return Task.CompletedTask;
        }
    };



});

//Service Providers
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileUploadService, FIleUploadService>();





// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JwtCleanArch API",
        Version = "v1",
        Description = "API using Clean Architecture + Identity + JWT"
    });

    // Optional: JWT Auth in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });

});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()     // this is the correct way
            .AllowAnyHeader()
            .AllowAnyMethod();
        // NO AllowCredentials here
    });
});


builder.Services.AddAuthorization();



var app = builder.Build();

var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/files"
});



// Configure Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JwtCleanArch API V1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Middleware
app.UseHttpsRedirection();

app.UseCors("CorsPolicy");       // must be after UseRouting


app.UseAuthentication(); // Needed for Identity/JWT
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity?.IsAuthenticated ?? false)
    {
        Console.WriteLine($"[AUTH] Request by: {user.Identity.Name}");
    }
    else
    {
        Console.WriteLine("[AUTH] Request is Anonymous");
    }
    await next();
});


app.MapControllers();



app.Run();
