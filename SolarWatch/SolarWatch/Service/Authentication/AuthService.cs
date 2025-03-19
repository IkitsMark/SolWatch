using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Service.Authentication;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<IdentityUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }
    //create new IdentityUser with email and username with CreateAsync (requireing password)
    //If it fails to create the user, return an error message with the email and username.
    //check if the role already exists
    //if the role does not exist, return an error message with invalid role
    //Add the user to the specified role
    //return the AuthResult with the email, username with a success
    public async Task<AuthResult> RegisterAsync(string email, string username, string password, string role)
    {
        var user = new IdentityUser { UserName = username, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return FailedRegistration(result, email, username);
        }

        var roleExists = await _userManager.IsInRoleAsync(user, role);
        if (!roleExists)
        {
            return new AuthResult(false, email, username, "")
            {
                ErrorMessages = { { "Invalid Role", $"The role '{role}' does not exist." } }
            };
        }


        await _userManager.AddToRoleAsync(user, role);
        return new AuthResult(true, email, username, "");
    }
    //On failiure, return AuthResult with an error message, the email and username
    private static AuthResult FailedRegistration(IdentityResult result, string email, string username)
    {
        var authResult = new AuthResult(false, email, username, "");

        foreach (var error in result.Errors)
        {
            authResult.ErrorMessages.Add(error.Code, error.Description);
        }

        return authResult;
    }
    //Find the user by email using FindByEmailAsync task
    //If the user is not found, return AuthResult w/ an error message with the email
    //Check if the password is valid using CheckPasswordAsync
    //If invalid, return AuthResult w/ an error message with the email and username
    //Get the roles of the user using GetRolesAsync
    //generate Token using CreateToken
    //on successreturn AuthResult with access token 
    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var managedUser = await _userManager.FindByEmailAsync(email);

        if (managedUser == null)
        {
            return InvalidEmail(email);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, password);
        if (!isPasswordValid)
        {
            return InvalidPassword(email, managedUser.UserName);
        }

        var roles = await _userManager.GetRolesAsync(managedUser);
        var role = roles.FirstOrDefault() ?? "User";

        var accessToken = _tokenService.CreateToken(managedUser, roles[0]);

        return new AuthResult(true, managedUser.Email, managedUser.UserName, accessToken);
    }
    //Create an Authresult for an invalid email scenario
    private static AuthResult InvalidEmail(string email)
    {
        var result = new AuthResult(false, email, "", "");
        result.ErrorMessages.Add("Bad credentials", "Invalid email");
        return result;
    }
    //Create an Authresult for an invalid password scenario
    private static AuthResult InvalidPassword(string email, string userName)
    {
        var result = new AuthResult(false, email, userName, "");
        result.ErrorMessages.Add("Bad credentials", "Invalid password");
        return result;
    }
    //Find the user by email using FindByEmailAsync task
    //if no user found, return false
    //retrive roles for the user
    //return true if user has admin role, otherwise false
    public async Task<bool> IsAdmin(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return false;
        var roles = await _userManager.GetRolesAsync(user);

        return roles.Contains("Admin");
    }
}