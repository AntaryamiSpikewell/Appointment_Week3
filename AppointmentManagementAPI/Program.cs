using AppointmentManagementAPI.Data;
using AppointmentManagementAPI.Repositories;
using AppointmentManagementAPI.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AppointmentManagementAPI.Mappings;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AppointmentManagementAPI.Services.Interfaces;
using AppointmentManagementAPI.Repositories.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories and Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IJwtService, JwtService>();


var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is missing!"));

// Configure Authentication and JWT Bearer
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true 
        };

        // Custom error message for missing/invalid token
        // Custom error handling
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = async context =>
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        message = context.Exception is SecurityTokenExpiredException
                            ? "Your session has expired. Please login again."
                            : "Authentication failed. Please check your token or login again."
                    });

                    await context.Response.WriteAsync(result);
                }
            },

            OnChallenge = async context =>
            {
                if (!context.Response.HasStarted)
                {
                    context.HandleResponse(); // Prevent default response
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        message = "Authentication token is missing or invalid. Please provide a valid token to access this resource."
                    });

                    await context.Response.WriteAsync(result);
                }
                else
                {
                    Console.WriteLine("Response already started before OnChallenge could execute.");
                }
            }
        };
    });


// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Appointment Management API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter '{token}' (without quotes). Example: 'eyJhbGciOiJIUzI1Ni...'"
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
            Array.Empty<string>()
        }
    });

    // Order Controllers in Swagger UI: Auth First, Appointments Second
    //c.OrderActionsBy((desc) => desc.GroupName);
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Enable Authentication & Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Run Application
app.Run();