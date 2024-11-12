using AutoMapper;
using AutoMapper.QueryableExtensions;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using CineManage.API.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace CineManage.API.Controllers;

[Route("api/genres")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Constants.AuthorizationIsAdmin)]
public class GenresController: StandardBaseController
{
    private readonly IOutputCacheStore _outputCacheStore;
    private readonly ApplicationContext _appContext;
    private readonly IMapper _mapper;
    private const string genresCacheTag = "genres";

    public GenresController(IOutputCacheStore outputCacheStore, ApplicationContext appContext,
        IMapper mapper) : base(appContext: appContext, mapper: mapper, outputCacheStore: outputCacheStore, cacheTag: genresCacheTag)
    {
        _outputCacheStore = outputCacheStore;
        _appContext = appContext;
        _mapper = mapper;
    }
    

    [HttpGet]
    [OutputCache(Tags = [genresCacheTag])]
    public async Task<List<GenreReadDTO>> Get([FromQuery] PaginationDTO pagination)
    {
        return await Get<Genre, GenreReadDTO>(paginationDTO: pagination, orderBy: genre => genre.Name);
    }

    [HttpGet("{id:int}", Name = "GetGenreById")] // api/genres/500
    [OutputCache(Tags = [genresCacheTag])]
    public async Task<ActionResult<GenreReadDTO>> Get(int id)
    {
        return await Get<Genre, GenreReadDTO>(id);
    }

    [HttpGet("all")]
    [OutputCache(Tags = [genresCacheTag])]
    [AllowAnonymous]
    public async Task<List<GenreReadDTO>> Get()
    {
        throw new Exception("For testing Application insights");
        return await Get<Genre, GenreReadDTO>(orderBy: genre => genre.Name);
    }
    
    [HttpPost]
    public async Task<CreatedAtRouteResult> Post([FromBody]GenreCreationDTO genreCreationDTO)
    {
        return await Post<GenreCreationDTO, Genre, GenreReadDTO>(genreCreationDTO, routeName: "GetGenreById");
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody]GenreCreationDTO genreCreationDTO)
    {
        return await Put<GenreCreationDTO, Genre>(id, genreCreationDTO);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await Delete<Genre>(id);
    }
}