using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SolarWatch.Service.Authentication;
using SolarWatch.Data;
using SolarWatch.Service;
using SolarWatch.Service.Repositories;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using SolarWatch.Services.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

public class Program
{
    //the main method now adheres to the S of the SOLID principles.
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();

        ConfigureServices(builder);
        ConfigureSwagger(builder);
        ConfigureDatabase(builder);
        ConfigureAuthentication(builder);
        ConfigureIdentity(builder);
        ConfigureCors(builder);

        var app = builder.Build();

        await ApplyMigrationsAndSeedDataAsync(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors();
        app.MapControllers();

        await app.RunAsync();
    }

    //creates a scoped instance of the service, in the framework of the Interfaces.
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<JwtSecurityTokenHandler>();

        builder.Services.AddScoped<IJsonProcessor, JsonProcessor>();
        builder.Services.AddScoped<IOpenWeather, OpenWeather>();
        builder.Services.AddScoped<ISunsetAndSunriseDataProvider, SunriseAndSunsetApi>();
        builder.Services.AddScoped<ICityRepository, CityRepository>();
        builder.Services.AddScoped<ISolarWatchRepository, SolarWatchRepository>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<AuthenticationSeeder>();
    }

    //Swagger configuration
    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "SolWatch API", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });
    }

    //gets connection string from appsettings.json and creates the DB based on the Contexts
    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("SolarApi");

        builder.Services.AddDbContext<SolarWatchContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddDbContext<UsersContext>(options => options.UseSqlServer(connectionString));


    }


    //this method configures the JWT token, appsettings has the specifics
    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var issuerSigningKey = builder.Configuration["Jwt:IssuerSigningKey"];
        if (string.IsNullOrWhiteSpace(issuerSigningKey))
        {
            throw new InvalidOperationException("JWT Issuer Signing Key is missing!");
        }
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["ValidIssuer"],
                    ValidAudience = jwtSettings["ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey))
                };
            });
    }

    //initializes EF Identity specifics
    private static void ConfigureIdentity(WebApplicationBuilder builder)
    {
        builder.Services
            .AddIdentityCore<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<UsersContext>();
    }

    private static void ConfigureCors(WebApplicationBuilder builder)
    {
        var frontendURL = builder.Configuration["URL:FrontendURL"];
        if (string.IsNullOrEmpty(frontendURL))
        {
            throw new ArgumentNullException("URL:FrontendURL", "The URL:FrontendURL configuration value is missing or empty.");
        }
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(frontendURL)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
    }

    private static async Task ApplyMigrationsAndSeedDataAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            var dbContext = new DbContext[]
            {
                scope.ServiceProvider.GetRequiredService<SolarWatchContext>(),
                scope.ServiceProvider.GetRequiredService<UsersContext>()
            };
            foreach(var context in dbContext)
            {
                await context.Database.MigrateAsync();
            }

            var authenticationSeeder = scope.ServiceProvider.GetRequiredService<AuthenticationSeeder>();
            await authenticationSeeder.AddRolesAsync();
            await authenticationSeeder.AddAdminAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database migration or seeding failed: {ex.Message}");
            throw;
        }
    }
}