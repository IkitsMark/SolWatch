using System
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
using SolarWatch.Data;
using SolarWatch.Service;
using SolarWatch.Service.Authentication;
using SolarWatch.Service.Repositories;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;


public class Program
{
    //the main method now adheres to the S of the SOLID principles.
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        AddServices(builder);
        ConfigureSwagger(builder);
        AddDbContexts(builder);
        AddAuthentication(builder);
        AddIdentity(builder);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        InitializeDb(app);
        SeedRolesAndAdminAsync(app).GetAwaiter().GetResult();

        app.Run();
    }

    //creates a scoped instance of the service, in the framework of the Interfaces.
    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddScoped<IJsonProcessor, JsonProcessor>();
        builder.Services.AddScoped<ISunsetAndSunriseDataProvider, SunriseAndSunSetApi>();
        builder.Services.AddScoped<ICityRepository, CityRepository>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
    }

    //Swagger configuration
    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
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
                    new string[]{}
                }
            });
        });
    }

    //gets connection string from appsettings.json and creates the DB based on teh Contexts
    private static void AddDbContexts(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Default");

        builder.Services.AddDbContext<SolarWatchContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddDbContext<UsersContext>(options => options.UseSqlServer(connectionString));
    }


    //this method configures the JWT token, appsettings has the specifics
    private static void AddAuthentication(WebApplicationBuilder builder)
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
    private static void AddIdentity(WebApplicationBuilder builder)
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

    //handles starting database migrations and runs PrintCities
    private static void InitializeDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SolarWatchContext>();
        db.Database.Migrate();
        PrintCities(db);
    }

    //method that prints to the console each city in the database
    private static void PrintCities(SolarWatchContext db)
    {
        foreach (var city in db.Cities)
        {
            Console.WriteLine($"{city.Id}, {city.Name}, {city.Latitude}, {city.Longitude}");
        }
    }

    //this seeds the roles and the admin if they are not found in the database. RoleManager is a scoped service, therefore we need a scope instance to access it
    private static async Task SeedRolesAndAdminAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await CreateRoleIfNotExists(roleManager, "Admin");
        await CreateRoleIfNotExists(roleManager, "User");
        await CreateAdminIfNotExists(userManager);
    }

    //this method creates roles if they don't exist
    private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    //This method creates an admin if it is unable to find it in the database
    private static async Task CreateAdminIfNotExists(UserManager<IdentityUser> userManager)
    {
        var adminEmail = "admin@admin.com";
        var adminInDb = await userManager.FindByEmailAsync(adminEmail);
        if (adminInDb == null)
        {
            var admin = new IdentityUser { UserName = "admin", Email = adminEmail };
            var result = await userManager.CreateAsync(admin, "admin123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}