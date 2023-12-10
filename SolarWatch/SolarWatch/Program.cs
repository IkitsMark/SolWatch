using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SolarWatch.Data;
using SolarWatch.Service;
using SolarWatch.Service.Authentication;
using SolarWatch.Service.Repositories;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

        builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

        addSwagger();
        builder.Services.AddDbContext<UsersContext>();
        builder.Services.AddScoped<IJsonProcessor, JsonProcessor>();
        builder.Services.AddScoped<ISunsetAndSunriseDataProvider, SunriseAndSunSetApi>();
        builder.Services.AddScoped<ICityRepository, CityRepository>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        addJwt();
        addIdentity();

        var app = builder.Build();

// Configure the HTTP request pipeline.
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

        InitializeDb();

        addRoles();

        AddAdmin();

        app.Run();


        void InitializeDb()
        {
            using var db = new SolarWatchContext();
            db.Database.EnsureCreated();
            PrintCities();

            void PrintCities()
            {
                    foreach (var city in db.Cities)
                    {
                        Console.WriteLine($"{city.Id}, {city.Name}, {city.Latitude}, {city.Longitude}");
                    }
            }
        }

        void addJwt()
        {
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "apiWithAuthBackend",
                        ValidAudience = "apiWithAuthBackend",
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes("!SomethingSecret!")
                        ),
                    };
                });
        }

        void addIdentity()
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

        void addSwagger()
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

        void addRoles()
        {
            using var scope = app.Services.CreateScope(); // RoleManager is a scoped service, therefore we need a scope instance to access it
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var tAdmin = CreateAdminRole(roleManager);
            tAdmin.Wait();

            var tUser = CreateUserRole(roleManager);
            tUser.Wait();
        }

        async Task CreateAdminRole(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole("Admin")); //The role string should better be stored as a constant or a value in appsettings
        }

        async Task CreateUserRole(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole("User")); //The role string should better be stored as a constant or a value in appsettings
        }

        void AddAdmin()
        {
            var tAdmin = CreateAdminIfNotExists();
            tAdmin.Wait();
        }

        async Task CreateAdminIfNotExists()
        {
            using var scope = app.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var adminInDb = await userManager.FindByEmailAsync("admin@admin.com");
            if (adminInDb == null)
            {
                var admin = new IdentityUser { UserName = "admin", Email = "admin@admin.com" };
                var adminCreated = await userManager.CreateAsync(admin, "admin123");

                if (adminCreated.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}