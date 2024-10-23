using AutoMapper;
using AutoMapper.QueryableExtensions;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CineManage.API.Controllers
{
    public class StandardBaseController : ControllerBase
    {

        private readonly ApplicationContext _appContext;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly string _cacheTag;

        public StandardBaseController(ApplicationContext appContext, IMapper mapper, 
            IOutputCacheStore outputCacheStore, string cacheTag)
        {
            _appContext = appContext;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
            _cacheTag = cacheTag;
        }

        protected async Task<List<TReadDTO>> Get<TEntity, TReadDTO>(PaginationDTO paginationDTO,
            Expression<Func<TEntity, object>> orderBy) 
            where TEntity: class
        {
            var queryableEntity = _appContext.Set<TEntity>().AsQueryable();

            await HttpContext.InserPaginationParametersInHeader(queryableEntity);

            return await queryableEntity.OrderBy(orderBy)
                .Paginate(paginationDTO)
                .ProjectTo<TReadDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        protected async Task<List<TReadDTO>> Get<TEntity, TReadDTO>(Expression<Func<TEntity, object>> orderBy)
            where TEntity : class
        {
            var queryableEntity = _appContext.Set<TEntity>().AsQueryable();
            
            return await queryableEntity.OrderBy(orderBy)
                .ProjectTo<TReadDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        protected async Task<ActionResult<TReadDto>> Get<TEntity, TReadDto>(int id)
            where TEntity: class
            where TReadDto: IId
        {
            var entityReadDto = await _appContext.Set<TEntity>().ProjectTo<TReadDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entityReadDto == null)
            {
                return NotFound();
            }

            return entityReadDto;
        }

        protected async Task<CreatedAtRouteResult> Post<TCreateDto, TEntity, TReadDto>(TCreateDto createDto, string routeName)
            where TEntity: class
            where TReadDto : IId
        
        {
            var entity = _mapper.Map<TEntity>(createDto);
           
            _appContext.Add(entity);
            await _appContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(_cacheTag, default);

            var readDto = _mapper.Map<TReadDto>(entity);

            return CreatedAtRoute(routeName, new { id = readDto.Id }, readDto);

        }

        protected async Task<IActionResult> Put<TCreateDto, TEntity>(int id, TCreateDto createDto)
            where TEntity : class, IId
        {
            bool doesEntityExist = await _appContext.Set<TEntity>().AnyAsync(t => t.Id == id);

            if (!doesEntityExist) 
            {
                return NotFound();
            }

            var entity = _mapper.Map<TEntity>(createDto);

            entity.Id = id;

            _appContext.Update(entity);

            await _appContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(_cacheTag, default);

            return NoContent();
        }

        protected async Task<IActionResult> Delete<TEntity>(int id)
            where TEntity:class, IId
        {
            var entity = await _appContext.Set<TEntity>().FindAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            _appContext.Set<TEntity>().Remove(entity);
            await _appContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(_cacheTag, default);

            return NoContent();
        }


    }
}
