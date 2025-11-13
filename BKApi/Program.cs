using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting.Server;

var builder = WebApplication.CreateBuilder(args);

// ==================== DATABASE ====================
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BK"),
        b => b.MigrationsAssembly("Infrastructure")
    )
);

// ==================== REPOSITORIES ====================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITourRepository, TourRepository>();
builder.Services.AddScoped<ITourImageRepository, TourImageRepository>();
builder.Services.AddScoped<ITourItineraryRepository, TourItineraryRepository>();
builder.Services.AddScoped<ITourIncludeRepository, TourIncludeRepository>();
builder.Services.AddScoped<ITourExcludeRepository, TourExcludeRepository>();
builder.Services.AddScoped<ITourTagRepository, TourTagRepository>();
builder.Services.AddScoped<ITourGuideRepository, TourGuideRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IDestinationRepository, DestinationRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IReviewImageRepository, ReviewImageRepository>();
builder.Services.AddScoped<IGuideRepository, GuideRepository>();
// Booking Repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingGuestRepository, BookingGuestRepository>();

// ==================== SERVICES ====================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IGuideService, GuideService>();
// Booking & Payment Services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured in appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($" Authentication Failed: {context.Exception.Message}");
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
            var role = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
            var name = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;

            Console.WriteLine($" Token Validated Successfully:");
            Console.WriteLine($"   - UserId: {userId}");
            Console.WriteLine($"   - Email: {email}");
            Console.WriteLine($"   - Name: {name}");
            Console.WriteLine($"   - Role: {role}");
            Console.WriteLine($"   - IsInRole(Admin): {context.Principal?.IsInRole("Admin")}");

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"⚠️ OnChallenge: {context.Error} - {context.ErrorDescription}");
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "You are not authorized to access this resource"
            });

            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            Console.WriteLine($"🚫 OnForbidden - User tried to access forbidden resource");
            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
            Console.WriteLine($"   - UserId: {userId}, Role: {role}");

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "You do not have permission to access this resource"
            });

            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("ManagerAndAbove", policy =>
        policy.RequireRole("Admin", "Manager"));

    options.AddPolicy("StaffAndAbove", policy =>
        policy.RequireRole("Admin", "Manager", "Staff"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Travel Booking API",
        Version = "v1",
        Description = "API for Travel Booking System with JWT Authentication & VNPay Payment Integration",
        Contact = new OpenApiContact
        {
            Name = "Travel Booking Team",
            Email = "support@travelbooking.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Booking API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Travel Booking API Documentation";
});

logger.LogInformation("Swagger UI enabled at: /swagger");

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;

        logger.LogError(exception, "Unhandled exception occurred");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            success = false,
            message = "An internal server error occurred",
            error = app.Environment.IsDevelopment() ? exception?.Message : null
        });

        await context.Response.WriteAsync(result);
    });
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();


app.Lifetime.ApplicationStarted.Register(() =>
{
    var addresses = app.Services.GetRequiredService<IServer>()
        .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()?
        .Addresses;

    logger.LogInformation("========================================");
    logger.LogInformation("Application Started Successfully!");
    logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
    logger.LogInformation("========================================");

    if (addresses != null)
    {
        foreach (var address in addresses)
        {
            logger.LogInformation(" Listening on: {Address}", address);
            logger.LogInformation(" Swagger UI: {Address}/swagger", address);
            logger.LogInformation(" Health Check: {Address}/health", address);
        }
    }

    // Log VNPay Configuration
    var vnpayTmnCode = app.Configuration["VnPay:TmnCode"];
    var vnpayUrl = app.Configuration["VnPay:Url"];
    logger.LogInformation("========================================");
    logger.LogInformation("💳 VNPay Configuration:");
    logger.LogInformation("   TmnCode: {TmnCode}", vnpayTmnCode ?? "Not Configured");
    logger.LogInformation("   URL: {Url}", vnpayUrl ?? "Not Configured");
    logger.LogInformation("   Status: {Status}", !string.IsNullOrEmpty(vnpayTmnCode) ? "Configured" : " Not Configured");
    logger.LogInformation("========================================");
});

app.Run();