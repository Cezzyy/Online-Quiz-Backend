using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OnlineQuiz.Data;
using OnlineQuiz.Services;
using OnlineQuiz.IServices;
using OnlineQuiz.Configuration;
using OnlineQuiz.Mappings;
using OnlineQuiz.IRepository;
using OnlineQuiz.Repository;
using Scalar.AspNetCore;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization for consistent API responses
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // Use camelCase
        options.JsonSerializerOptions.WriteIndented = true; // Pretty print in development
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configure Entity Framework
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OnlineQuizDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure JWT from environment variables
var jwtSettings = new JwtSettings
{
    SecretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey") ?? "",
    Issuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer") ?? "OnlineQuizAPI",
    Audience = Environment.GetEnvironmentVariable("JwtSettings__Audience") ?? "OnlineQuizUsers",
    AccessTokenExpirationInMinutes = int.TryParse(Environment.GetEnvironmentVariable("JwtSettings__AccessTokenExpirationInMinutes"), out var accessExpiration) ? accessExpiration : 15,
    RefreshTokenExpirationInDays = int.TryParse(Environment.GetEnvironmentVariable("JwtSettings__RefreshTokenExpirationInDays"), out var refreshExpiration) ? refreshExpiration : 7
};

// Validate JWT settings
if (string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured. Please set JwtSettings__SecretKey environment variable.");
}

builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = jwtSettings.SecretKey;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.AccessTokenExpirationInMinutes = jwtSettings.AccessTokenExpirationInMinutes;
    options.RefreshTokenExpirationInDays = jwtSettings.RefreshTokenExpirationInDays;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "")),
            ClockSkew = TimeSpan.Zero // Reduce clock skew for mobile
        };

        // Support both Authorization header (mobile) and cookies (web)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Check for token in Authorization header first (mobile)
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    return Task.CompletedTask;
                }

                // Check for token in cookies (web)
                var token = context.Request.Cookies["__Host-jwt"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

// Add AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfile));

// Register Repository Layer
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
// builder.Services.AddScoped<ICourseRepository, CourseClass>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

// Register Service Layer
builder.Services.AddScoped<IUserService, UserService>();
// builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

// Configure CORS for Web (Vue) and Mobile (Flutter)
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:8080",
                "http://localhost:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();            // Allow cookies for web
    });

    options.AddPolicy("MobilePolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Authorization");
    });

    // Combined policy for both
    options.AddPolicy("AllowWebAndMobile", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
            {
                // Allow localhost for development
                if (string.IsNullOrEmpty(origin)) return true;
                if (origin.StartsWith("http://localhost") || origin.StartsWith("https://localhost")) return true;
                // Add your production domains here
                return false;
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Authorization", "Content-Type", "X-Total-Count");
    });
});

// Configure services for Scalar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "OnlineQuiz API", 
        Version = "v1",
        Description = "A comprehensive online quiz platform API"
    });
    
    // Configure JWT Bearer authentication
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference(options =>
    {
        options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowWebAndMobile");

// Add security headers
app.Use(async (context, next) =>
{
    // Security headers for web clients
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    // API-specific headers
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Redirect root path to Scalar API documentation
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.MapControllers();

app.Run();
