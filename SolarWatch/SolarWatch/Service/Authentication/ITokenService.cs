// 

using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Service.Authentication;

public interface ITokenService
{
    public string CreateToken(IdentityUser user,string role);
}