using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Services.Authentication;
public class AuthenticationSeeder
{
    private RoleManager<IdentityRole> roleManager;
    private UserManager<IdentityUser> userManager;
    private readonly IConfiguration _configuration;
    //constructor for the AuthenticationSeeder, user and role managers are separated
    public AuthenticationSeeder(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
        _configuration = configuration;
    }
    //sync method to create roles using async tasks
    public async Task AddRolesAsync()
    {
        await CreateAdminRole(roleManager);
        await CreateUserRole(roleManager);
    }
    // creates the admin role, async wth the RoleManager of Identity
    private async Task CreateAdminRole(RoleManager<IdentityRole> roleManager)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    // creates the user role, async wth the RoleManager of Identity
    async Task CreateUserRole(RoleManager<IdentityRole> roleManager)
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    }
    //ensure to add admin is created if none exist already, waits for it to complete sync
    public async Task AddAdminAsync()
    {
        await CreateAdminIfNotExists();
    }
    //the method to create an admin if none exist in the database, retrieves email, username and password from config
    //check if it already exists or not, if not create a new IdentityUser with the config specifics.
    //If the user is created, then add the role "Admin" to the user.
    private async Task CreateAdminIfNotExists()
    {
        var adminEmail = _configuration["Admin:Email"];
        if (string.IsNullOrEmpty(adminEmail))
        {
            throw new ArgumentNullException("Admin: Email configuration is missing or empty.");
        }
        var adminUsername = _configuration["Admin:Username"];
        var adminPassword = _configuration["Admin:Password"];


        var adminInDb = await userManager.FindByEmailAsync(adminEmail);
        if (adminInDb == null)
        {
            var admin = new IdentityUser { UserName = adminUsername, Email = adminEmail };
            var adminCreated = await userManager.CreateAsync(admin, adminPassword);

            if (adminCreated.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}