using Microsoft.AspNetCore.Identity;

namespace CineManage.API.Services;

public class UsersService : IUsersService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager;

    public UsersService(IHttpContextAccessor httpContextAccessor,
        UserManager<IdentityUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }
    public async Task<string?> GetUserId()
    {

        var emailFromClaim = _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == "email")?.Value;

        if (string.IsNullOrEmpty(emailFromClaim))
        {
            return null;
        }

        var user = await _userManager.FindByEmailAsync(emailFromClaim);

        return user?.Id;
    }
}