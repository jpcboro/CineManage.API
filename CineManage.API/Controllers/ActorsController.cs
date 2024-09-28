using AutoMapper;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace CineManage.API.Controllers
{
    [Route("api/actors")]
    [ApiController]
    public class ActorsController : ControllerBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string actorsCacheTag = "actors";

        public ActorsController(ApplicationContext appContext, IMapper mapper, IOutputCacheStore outputCacheStore)
        {
            _appContext = appContext;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] ActorCreationDTO actorCreationDTO)
        {
            Actor actor = _mapper.Map<Actor>(actorCreationDTO);

            _appContext.Add(actor);

            await _appContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(actorsCacheTag, default);

            return Ok();

        }
    }
}
