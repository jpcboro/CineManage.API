using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using CineManage.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineManage.API.Controllers;

[ApiController]
[Route("api/ratings")]
public class RatingsController : ControllerBase
{
    private readonly ApplicationContext _appContext;
    private readonly IUsersService _usersService;

    public RatingsController(ApplicationContext appContext,
        IUsersService usersService)
    {
        _appContext = appContext;
        _usersService = usersService;
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Post([FromBody] RatingCreationDTO ratingCreationDto)
    {
        bool movieExists = await _appContext.Movies
            .AnyAsync(m => m.Id == ratingCreationDto.MovieId);

        if (!movieExists)
        {
            return NotFound();
        }

        string userId = await _usersService.GetUserId() ?? string.Empty;

        var currentRating = await _appContext.MovieRatings
            .FirstOrDefaultAsync(r => r.MovieId == ratingCreationDto.MovieId
                                 && r.UserId == userId);

        if (currentRating == null && !string.IsNullOrEmpty(userId))
        {
            Rating newRating = new Rating()
            {
                MovieId = ratingCreationDto.MovieId,
                Rate = ratingCreationDto.Rate,
                UserId = userId
            };

            _appContext.Add(newRating);
        }
        else
        {
            currentRating.Rate = ratingCreationDto.Rate;
        }

        await _appContext.SaveChangesAsync();

        return NoContent();
    }
}