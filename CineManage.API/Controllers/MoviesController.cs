using AutoMapper;
using AutoMapper.QueryableExtensions;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using CineManage.API.Services;
using CineManage.API.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace CineManage.API.Controllers
{
    [Route("api/movies")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MoviesController : ControllerBase
    {
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IFileStorage _fileStorage;
        private readonly IUsersService _usersService;
        private readonly ApplicationContext _appContext;
        private readonly IMapper _mapper;
        private const string moviesCacheTag = "movies";
        private const string moviesContainer = "movies";
        private const string getMovieByIdRouteName = "GetMovieById";

        public MoviesController(ApplicationContext appContext, IMapper mapper,
            IOutputCacheStore outputCacheStore, IFileStorage fileStorage,
            IUsersService usersService)
        {
            _appContext = appContext;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
            _fileStorage = fileStorage;
            _usersService = usersService;
        }

        [HttpGet("{id:int}", Name = getMovieByIdRouteName)]
        [OutputCache(Tags = [moviesCacheTag])]
        [AllowAnonymous]
        public async Task<ActionResult<MovieDetailsDTO>> Get(int id)
        {
            var movie = await _appContext.Movies
                                         .ProjectTo<MovieDetailsDTO>(_mapper.ConfigurationProvider)
                                         .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var ratings = _appContext.MovieRatings.Where(r => r.MovieId == id);
            
            movie.AverageRating = await ratings.AnyAsync()
                ? await ratings.AverageAsync(r => r.Rate)
                : 0.0;


            if (HttpContext.User.Identity?.IsAuthenticated != true)
            {
                return movie;
            }
            
      
            string? userId = await _usersService.GetUserId();

            movie.UserRating = !string.IsNullOrEmpty(userId)
                ? await ratings.Where(r => r.UserId == userId)
                    .Select(r => r.Rate)
                    .FirstOrDefaultAsync()
                : 0;
            
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

        [HttpGet("home")]
        [OutputCache(Tags = [moviesCacheTag])]
        [AllowAnonymous]
        public async Task<ActionResult<HomePageDTO>> Get()
        {
            var dateToday = DateTime.Today;
            var topNumber = 6;

            var upcomingMovies = await _appContext.Movies.Where(m => m.ReleaseDate > dateToday)
                .OrderBy(m => m.ReleaseDate)
                .Take(topNumber)
                .ProjectTo<MovieReadDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var nowShowing = await _appContext.Movies.Where(m => m.CinemaScreenings.Select(c => c.MovieId)
                    .Contains(m.Id))
                .OrderBy(m => m.ReleaseDate)
                .Take(topNumber)
                .ProjectTo<MovieReadDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var result = new HomePageDTO()
            {
                NowShowing = nowShowing,
                UpcomingMovies = upcomingMovies
            };

            return result;
        }
        
        [HttpGet("putget/{id:int}")]
        public async Task<ActionResult<MoviesPutGetOptionsDTO>> PutGet(int id)
        {
            var movieDetailsDto = await _appContext.Movies.ProjectTo<MovieDetailsDTO>(_mapper.ConfigurationProvider)
                                        .FirstOrDefaultAsync(m => m.Id == id);

            if (movieDetailsDto == null)
            {
                return NotFound();
            }
            
            var selectedGenreIds = movieDetailsDto.Genres.Select(g => g.Id);
            
            var nonSelectedGenres = await _appContext.Genres.Where(g => !selectedGenreIds.Contains(g.Id))
                .ProjectTo<GenreReadDTO>(_mapper.ConfigurationProvider).ToListAsync();
            

            var selectedMovieTheaterIds = movieDetailsDto.MovieTheaters.Select(m => m.Id);
            

            var nonSelectedTheaters = await _appContext.MovieTheaters
                .Where(t => !selectedMovieTheaterIds.Contains(t.Id))
                .ProjectTo<MovieTheaterReadDTO>(_mapper.ConfigurationProvider).ToListAsync();


            return new MoviesPutGetOptionsDTO()
            {
                Movie = movieDetailsDto,
                SelectedGenres = movieDetailsDto.Genres,
                NonSelectedGenres = nonSelectedGenres,
                SelectedTheaters = movieDetailsDto.MovieTheaters,
                NonSelectedTheaters = nonSelectedTheaters,
                Actors = movieDetailsDto.Actors
            };
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDTO movieCreationDto)
        {
            
            var movie = await _appContext.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.CinemaScreenings)
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == id);

            
            if (movie == null)
            {
                return NotFound();
            }

            _mapper.Map(movieCreationDto, movie);

            if (movieCreationDto.Poster != null)
            {
                movie.Poster = await _fileStorage.SaveEditedFile(movie.Poster,
                    moviesContainer, movieCreationDto.Poster);
            }
            
            AssignActorsCreditsOrder(movie);
            
            await _appContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(moviesCacheTag, default);

            return NoContent();

        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
              var movie = await _appContext.Movies.FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            _appContext.Remove(movie);
            await _appContext.SaveChangesAsync();

            await _fileStorage.Delete(route: movie.Poster, container: moviesContainer);
            await _outputCacheStore.EvictByTagAsync(moviesCacheTag, default);

            return NoContent();
            
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<List<MovieReadDTO>> Get([FromQuery]MoviesFilterDTO moviesFilterDto)
        {
            var moviesQueryable = _appContext.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(moviesFilterDto.Title))
            {
                moviesQueryable = moviesQueryable
                    .Where(m => m.Title.ToLower()
                        .Contains(moviesFilterDto.Title.ToLower()));

            }

            if (moviesFilterDto.IsNowShowing)
            {
                moviesQueryable = moviesQueryable.Where(movie => movie.CinemaScreenings.Select(c => c.MovieId).Contains(movie.Id));
            }

            if (moviesFilterDto.IsUpcomingMovie)
            {
                var dateToday = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(movie => movie.ReleaseDate > dateToday);
            }

            if (moviesFilterDto.GenreId != 0)
            {
                moviesQueryable = moviesQueryable.Where(movie => movie.MovieGenres.Select(mg => mg.GenreId)
                    .Contains(moviesFilterDto.GenreId));
            }

            await HttpContext.InserPaginationParametersInHeader(moviesQueryable);

            var movies = await moviesQueryable.Paginate(moviesFilterDto.PaginationDto)
                .ProjectTo<MovieReadDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return movies;
        }
        
        
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
