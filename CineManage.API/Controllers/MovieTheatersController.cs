using AutoMapper;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace CineManage.API.Controllers
{
    [Route("api/movietheaters")]
    [ApiController]
    public class MovieTheatersController : StandardBaseController
    {
        private const string mTheaterCacheTag = "movieTheaters";
        private readonly ApplicationContext _appContext;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;

        public MovieTheatersController(ApplicationContext appContext, IMapper mapper,
            IOutputCacheStore outputCacheStore)  
           : base(appContext: appContext, mapper: mapper, outputCacheStore: outputCacheStore, cacheTag: mTheaterCacheTag)
        {
            _appContext = appContext;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        [OutputCache(Tags = [mTheaterCacheTag])]
        public async Task<List<MovieTheaterReadDTO>> Get([FromQuery] PaginationDTO pagination)
        {
            return await Get<MovieTheater, MovieTheaterReadDTO>(paginationDTO: pagination, orderBy: mTheater => mTheater.Name);
        }

        [HttpGet("{id:int}", Name = "GetMovieTheaterById")]
        [OutputCache(Tags = [mTheaterCacheTag])]
        public async Task<ActionResult<MovieTheaterReadDTO>> Get(int id)
        {
            return await Get<MovieTheater, MovieTheaterReadDTO>(id);
        }

        [HttpPost]
        public async Task<CreatedAtRouteResult> Post([FromBody] MovieTheaterCreationDTO mTheaterCreationDTO)
        {
            return await Post<MovieTheaterCreationDTO, MovieTheater, 
                MovieTheaterReadDTO>(mTheaterCreationDTO, routeName: "GetMovieTheaterById");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] MovieTheaterCreationDTO mTheaterCreationDTO)
        {
            return await Put<MovieTheaterCreationDTO, MovieTheater>(id, mTheaterCreationDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await Delete<MovieTheater>(id);
        }
    }
}
