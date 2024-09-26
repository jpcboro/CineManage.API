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
    public async Task<ActionResult<GenreReadDTO>> Get(int id)
    {
        GenreReadDTO? genre = await _appContext.Genres
             .ProjectTo<GenreReadDTO>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(g => g.Id == id);

        if (genre is null)
        {
            return NotFound();
        }

        return genre;

    }
    
    [HttpPost]
    public async Task<CreatedAtRouteResult> Post([FromBody]GenreCreationDTO genreCreationDTO)
    {
        Genre genre = _mapper.Map<Genre>(genreCreationDTO);

        await _outputCacheStore.EvictByTagAsync(genresCacheTag, default);
        _appContext.Add(genre);
        await _appContext.SaveChangesAsync();

        var genreDTO = _mapper.Map<GenreReadDTO>(genre);

        return  CreatedAtRoute("GetGenreById", new { id = genreDTO.Id }, genreDTO);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody]GenreCreationDTO genreCreationDTO)
    {
        bool doesGenreExist = await _appContext.Genres.AnyAsync(g => g.Id == id);

        if (!doesGenreExist)
        {
            return NotFound();
        }

        Genre genre = _mapper.Map<Genre>(genreCreationDTO);

        genre.Id = id;

        _appContext.Update(genre);

        await _appContext.SaveChangesAsync();

        await _outputCacheStore.EvictByTagAsync(genresCacheTag, default);

        return NoContent();


    }

    [HttpDelete]
    public ActionResult Delete()
    {
        return NoContent();
    }
}