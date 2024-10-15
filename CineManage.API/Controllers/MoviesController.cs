using AutoMapper;
using AutoMapper.QueryableExtensions;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using CineManage.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace CineManage.API.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IFileStorage _fileStorage;
        private readonly ApplicationContext _appContext;
        private readonly IMapper _mapper;
        private const string moviesCacheTag = "movies";
        private const string moviesContainer = "movies";
        private const string getMovieByIdRouteName = "GetMovieById";

        public MoviesController(ApplicationContext appContext, IMapper mapper,
            IOutputCacheStore outputCacheStore, IFileStorage fileStorage)
        {
            _appContext = appContext;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
            _fileStorage = fileStorage;
        }

        [HttpGet("{id:int}", Name = getMovieByIdRouteName)]
        [OutputCache(Tags = [moviesCacheTag])]
        public async Task<ActionResult<MovieReadDTO>> Get(int id)
        {
            var movie = await _appContext.Movies.ProjectTo<MovieReadDTO>(_mapper.ConfigurationProvider)
                                                          .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return movie;
        }
        
        [HttpGet("postget")]
        public async Task<ActionResult<MoviesPostGetOptionsDTO>> PostGet()
        {
            var genres = await _appContext.Genres.OrderBy(g => g.Name).ProjectTo<GenreReadDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var movieTheaters = await _appContext.MovieTheaters.OrderBy(t => t.Name).ProjectTo<MovieTheaterReadDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new MoviesPostGetOptionsDTO { Genres = genres, MoviesTheaters = movieTheaters };

        }

        [HttpPost]
        public async Task<CreatedAtRouteResult> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            Movie movie = _mapper.Map<Movie>(movieCreationDTO);

            if (movieCreationDTO.Poster != null)
            {
                string posterUrl = await _fileStorage.SaveFile(container: moviesContainer, file: movieCreationDTO.Poster);

                movie.Poster = posterUrl;
            }

            AssignActorsCreditsOrder(movie);

            _appContext.Add(movie);

            await _appContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(moviesCacheTag, default);

            MovieReadDTO movieReadDTO = _mapper.Map<MovieReadDTO>(movie);

            return CreatedAtRoute(getMovieByIdRouteName, new { Id = movie.Id }, movieReadDTO);
        }

        //[HttpGet("putget/{id:int}")]
        //public async Task<ActionResult<MoviesPutGetOptionsDTO>> PutGet(int id)
        //{
        //    var movie = await _appContext.Movies.ProjectTo<MovieDetailsDTO>(_mapper.ConfigurationProvider)
        //        .FirstOrDefaultAsync(mdDTO => mdDTO.Id == id);

        //    if (movie == null)
        //    {
        //        return NotFound();
        //    }


        //}


        private void AssignActorsCreditsOrder(Movie movie)
        {
            if (movie.MovieActors is not null)
            {
                for (int i = 0; i < movie.MovieActors.Count; i++)
                {
                    movie.MovieActors[i].Order = i;
                }
            }
        }
    }
}
