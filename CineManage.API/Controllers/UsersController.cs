using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using CineManage.API.Data;
using CineManage.API.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.IdentityModel.Tokens;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace CineManage.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Constants.AuthorizationIsAdmin)]
public class UsersController : StandardBaseController
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly ApplicationContext _appContext;
    private readonly IOutputCacheStore _outputCacheStore;
    private readonly IMapper _mapper;
    private const string createAdmin = "createAdmin";
    private const string removeAdmin = "removeAdmin";
    private const string trueString = "true";
    private const string usersCacheTag = "users";

    public UsersController(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration config, ApplicationContext appContext, IOutputCacheStore outputCacheStore,
        IMapper mapper) : base(appContext: appContext, mapper: mapper, outputCacheStore: outputCacheStore,
        cacheTag: usersCacheTag)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _appContext = appContext;
        _outputCacheStore = outputCacheStore;
        _mapper = mapper;
    }

    [HttpPost("register")]
    [AllowAnonymous]
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
    [AllowAnonymous]
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

    [HttpPost(createAdmin)]
    public async Task<IActionResult> CreateAdmin(EditClaimDTO editClaimDto)
    {
        var user = await _userManager.FindByEmailAsync(editClaimDto.Email);

        if (user == null)
        {
            return NotFound();
        }

        await _userManager.AddClaimAsync(user, new Claim(type: Constants.AuthorizationIsAdmin,
            value: trueString));

        return NoContent();
    }

    [HttpPost(removeAdmin)]
    
    public async Task<IActionResult> RemoveAdmin(EditClaimDTO editClaimDto)
    {
        var user = await _userManager.FindByEmailAsync(editClaimDto.Email);

        if (user == null)
        {
            return NotFound();
        }

        await _userManager.RemoveClaimAsync(user, new Claim(Constants.AuthorizationIsAdmin, trueString));

        return NoContent();
    }
    

    [HttpGet("usersAndAdminsList")]
    [OutputCache(Tags = [usersCacheTag])]
    public async Task<ActionResult<List<UserDTO>>> GetAllUsersAndAdminStatuses([FromQuery] PaginationDTO paginationDto)
    {
        var users = await Get<IdentityUser, UserDTO>(paginationDto, orderBy: u => u.Email!);

        var userAdminStatuses = new List<UserDTO>();

        foreach (var userDto in users)
        {
            var user = await _userManager.FindByEmailAsync(userDto.Email);
            
            if (user == null) 
                continue;

            var claims = await _userManager.GetClaimsAsync(user);
            userDto.IsAdmin = claims.Any(c => c.Type == Constants.AuthorizationIsAdmin && c.Value == "true");
            
            userAdminStatuses.Add(userDto);
        }

        return Ok(userAdminStatuses);
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