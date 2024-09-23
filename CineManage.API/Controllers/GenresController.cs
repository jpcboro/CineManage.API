using CineManage.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace CineManage.API.Controllers;

[Route("api/genres")]
[ApiController]
public class GenresController: ControllerBase
{
    private readonly IOutputCacheStore _outputCacheStore;
    private const string genresCacheTag = "genres";

    public GenresController(IOutputCacheStore outputCacheStore)
    {
        _outputCacheStore = outputCacheStore;
    }
    

    [HttpGet]
    [OutputCache(Tags = [genresCacheTag])]
    public List<Genre> Get()
    {
        return new List<Genre>
        {
            new Genre { Id = 1, Name = "Action" },
            new Genre { Id = 1, Name = "Comedy" },
            new Genre { Id = 1, Name = "Drama" },
        };
    }

    [HttpGet("{id:int}")] // api/genres/500
    [OutputCache(Tags = [genresCacheTag])]
    public Task<ActionResult<Genre>> Get(int id)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<ActionResult<Genre>> Post([FromBody]Genre genre)
    {
        await _outputCacheStore.EvictByTagAsync("genres", default);
        throw new NotImplementedException();
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