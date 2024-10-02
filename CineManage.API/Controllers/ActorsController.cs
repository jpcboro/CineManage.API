using AutoMapper;
using AutoMapper.QueryableExtensions;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using CineManage.API.Services;
using CineManage.API.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace CineManage.API.Controllers
{
    [Route("api/actors")]
    [ApiController]
    public class ActorsController : ControllerBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IFileStorage _fileStorage;
        private const string actorsCacheTag = "actors";
        private readonly string actorsContainer = "actors";

        public ActorsController(ApplicationContext appContext, IMapper mapper, IOutputCacheStore outputCacheStore,
            IFileStorage fileStorage)
        {
            _appContext = appContext;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
            _fileStorage = fileStorage;
        }

        [HttpGet]
        [OutputCache(Tags = [actorsCacheTag])]
        public async Task<List<ActorReadDTO>> Get([FromQuery] PaginationDTO pagination)
        {
            var queryable = _appContext.Actors;
           
            await HttpContext.InserPaginationParametersInHeader(queryable);
            
            return await queryable.OrderBy(a => a.Name)
                            .Paginate(pagination)
                            .ProjectTo<ActorReadDTO>(_mapper.ConfigurationProvider)
                            .ToListAsync();
        }

        [HttpGet("{id:int}", Name = "GetActorById")]
        [OutputCache(Tags = [actorsCacheTag])]
        public async Task<ActionResult<ActorReadDTO>> Get(int id)
        {
            var actor = await _appContext.Actors.ProjectTo<ActorReadDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            return actor;

        }

        [HttpPost]
        public async Task<CreatedAtRouteResult> Post([FromForm] ActorCreationDTO actorCreationDTO)
        {
            Actor actor = _mapper.Map<Actor>(actorCreationDTO);

            if (actorCreationDTO.Picture != null)
            {
                string pictureUrl = await _fileStorage.SaveFile(container: actorsContainer, file: actorCreationDTO.Picture);

                actor.Picture = pictureUrl;
            }

            _appContext.Add(actor);

            await _appContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(actorsCacheTag, default);

            var actorReadDto = _mapper.Map<ActorReadDTO>(actor);

            return CreatedAtRoute("GetActorById", new { Id = actor.Id}, actorReadDto);

        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromForm]ActorCreationDTO actorCreateDTO)
        {
            var actor = await _appContext.Actors.FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            actor = _mapper.Map(actorCreateDTO, actor);

            if (actorCreateDTO.Picture != null)
            {
                actor.Picture = await _fileStorage.SaveEditedFile(actor.Picture, actorsContainer, actorCreateDTO.Picture);
            }

            await _appContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(actorsCacheTag, default);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
           
            var actor = await _appContext.Actors.FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            _appContext.Remove(actor);
            await _appContext.SaveChangesAsync();

            await _fileStorage.Delete(actor.Picture, actorsContainer);

            await _outputCacheStore.EvictByTagAsync(actorsCacheTag, default);

            return NoContent();
        }
    }
}
