using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CineManage.API.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace CineManage.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;

    public UsersController(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResponseDTO>> Register(UserLoginDTO userLoginDto)
    {
        IdentityUser user = new IdentityUser()
        {
            UserName = userLoginDto.Email,
            Email = userLoginDto.Email
        };

        IdentityResult result = await _userManager
            .CreateAsync(user, userLoginDto.Password);

        if (result.Succeeded)
        {
            return await CreateTokenFromUser(user);
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponseDTO>> Login(UserLoginDTO userLoginDto)
    {
        IdentityUser? user = await _userManager.FindByEmailAsync(userLoginDto.Email);

        if (user is null)
        {
            var errors = CreateWrongLoginErrorMessage();
            return BadRequest(errors);
        }

        SignInResult result = await _signInManager.CheckPasswordSignInAsync(user: user,
            password: userLoginDto.Password,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return await CreateTokenFromUser(user);
        }
        else
        {
            var errors = CreateWrongLoginErrorMessage();
            return BadRequest(errors);
        }
    }

    private IEnumerable<IdentityError> CreateWrongLoginErrorMessage()
    {
        return new[]
        {
            new IdentityError()
            {
                Description = "Login is Incorrect."
            }
        };
    }
    
    

    private async Task<AuthenticationResponseDTO> CreateTokenFromUser(IdentityUser user)
    {
        var claimsList = new List<Claim>
        {
            new Claim("email", user.Email!)
        };

        var additionalClaims = await _userManager.GetClaimsAsync(user);
        
        claimsList.AddRange(additionalClaims);

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_config["jwtKey"]!));

        SigningCredentials credentials = new SigningCredentials(key,
            SecurityAlgorithms.HmacSha256);
        
        DateTime expiryDate = DateTime.UtcNow.AddYears(1);

        JwtSecurityToken securityToken = new JwtSecurityToken(issuer: null,
            audience: null, claims: claimsList, expires: expiryDate, 
            signingCredentials: credentials);

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        string tokenString = tokenHandler.WriteToken(securityToken);

        return new AuthenticationResponseDTO()
        {
            Token = tokenString,
            Expiration = expiryDate
        };
    }
    
    
    
    
}