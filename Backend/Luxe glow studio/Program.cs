using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Luxe_glow_studio.Data;
using Luxe_glow_studio.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

// CORS configuration for Frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "LuxeGlowStudio_SuperSecret_JwtKey_2026!#$Secure";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LuxeGlowStudio";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LuxeGlowStudioUsers";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Luxe Glow Studio API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
        c.DocumentTitle = "Luxe Glow Studio - Professional API Documentation";
        c.DefaultModelExpandDepth(2);
        c.DefaultModelsExpandDepth(-1);
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
        c.SupportedSubmitMethods(new[] { 
            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get, 
            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post, 
            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put, 
            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete,
            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Patch
        });
        
        // Custom CSS for professional look
        c.InjectStylesheet("/swagger-ui/custom.css");
    });
}

// Important: UseCors must be before Authentication, Authorization, and Redirection
app.UseCors("AllowAll");

// Enable serving static files (for custom CSS)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();