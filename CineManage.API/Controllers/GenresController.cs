using AutoMapper;
using AutoMapper.QueryableExtensions;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using CineManage.API.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace CineManage.API.Controllers;

[Route("api/genres")]
[ApiController]
public class GenresController: ControllerBase
{
    private readonly IOutputCacheStore _outputCacheStore;
    private readonly ApplicationContext _appContext;
    private readonly IMapper _mapper;
    private const string genresCacheTag = "genres";

    public GenresController(IOutputCacheStore outputCacheStore, ApplicationContext appContext,
        IMapper mapper)
    {
        _outputCacheStore = outputCacheStore;
        _appContext = appContext;
        _mapper = mapper;
    }
    

    [HttpGet]
    [OutputCache(Tags = [genresCacheTag])]
    public async Task<List<GenreReadDTO>> Get([FromQuery] PaginationDTO pagination)
    {
        IQueryable<Genre> queryableGenres = _appContext.Genres;

        await HttpContext.InserPaginationParametersInHeader(queryableGenres);

        return await queryableGenres
            .OrderBy(i => i.Name)
            .Paginate(pagination)
            .ProjectTo<GenreReadDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    [HttpGet("{id:int}", Name = "GetGenreById")] // api/genres/500
    [OutputCache(Tags = [genresCacheTag])]
    public Task<ActionResult<Genre>> Get(int id)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<CreatedAtRouteResult> Post([FromBody]GenreCreationDTO genreCreationDTO)
    {
        var genre = _mapper.Map<Genre>(genreCreationDTO);

        await _outputCacheStore.EvictByTagAsync("genres", default);
        _appContext.Add(genre);
        await _appContext.SaveChangesAsync();

        var genreDTO = _mapper.Map<GenreReadDTO>(genre);

        return  CreatedAtRoute("GetGenreById", new { id = genreDTO.Id }, genreDTO);
    }

    [HttpPut]
    public ActionResult Put([FromBody]Genre genre)
    {
        return NoContent();
    }

    [HttpDelete]
    public ActionResult Delete()
    {
        return NoContent();
    }
}