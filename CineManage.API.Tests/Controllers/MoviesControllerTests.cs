using AutoMapper;
using CineManage.API.Controllers;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using CineManage.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;

namespace CineManage.API.Tests.Controllers
{
    public class MoviesControllerTests
    {
        private readonly Mock<IOutputCacheStore> _mockOutputCacheStore;
        private readonly ApplicationContext _appContext;
        private readonly MoviesController _controller;
        private readonly Mock<IFileStorage> _mockFileStorage;

        public MoviesControllerTests()
        {
            _mockOutputCacheStore = new Mock<IOutputCacheStore>();

            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _appContext = new ApplicationContext(options);
        
            SeedData();
            
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Movie, MovieDetailsDTO>()
                    .ForMember(dest => dest.Genres,
                        opt =>
                            opt.MapFrom(src => src.MovieGenres.Select(
                                mg => new GenreReadDTO()
                                {
                                    Id = mg.GenreId,
                                    Name = mg.Genre.Name
                                })))
                    .ForMember(dest => dest.MovieTheaters, opt =>
                        opt.MapFrom(src => src.CinemaScreenings.Select(
                            cs => new MovieTheaterReadDTO
                            {
                                Id = cs.MovieTheaterId,
                                Name = cs.MovieTheater.Name
                            }
                        )))
                    .ForMember(dest => dest.Actors, opt =>
                        opt.MapFrom(src => src.MovieActors.Select(ma => new MovieActorReadDTO()
                        {
                            Id = ma.ActorId,
                            Name = ma.Actor.Name
                        })));
                    ;

                cfg.CreateMap<Genre, GenreReadDTO>();
                cfg.CreateMap<MovieTheater, MovieTheaterReadDTO>();
                cfg.CreateMap<Actor, MovieActorReadDTO>();
                cfg.CreateMap<Movie, MovieReadDTO>();
                cfg.CreateMap<MovieCreationDTO, Movie>();
            });

            var mockMapper = mapperConfig.CreateMapper();

            _mockFileStorage = new Mock<IFileStorage>();

            _controller = new MoviesController(_appContext, mockMapper,
                _mockOutputCacheStore.Object, _mockFileStorage.Object);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private void SeedData()
        {
            var genre1 = new Genre { Id = 1, Name = "Science Fiction" };
            var genre2 = new Genre { Id = 2, Name = "Action" };

            _appContext.Genres.AddRange(genre1, genre2);

            var movieTheater1 = new MovieTheater
            {
                Id = 1,
                Name = "Carlton Indie",
                Location = new Point(-37.805508, 144.971445)
                {
                    SRID = 4326 
                }
            };

            var movieTheater2 = new MovieTheater
            {
                Id = 2,
                Name = "Royal Botanical Gardens Theatre",
                Location = new Point(-37.828806, 144.980058)
                {
                    SRID = 4326
                }
            };

            _appContext.MovieTheaters.AddRange(movieTheater1, movieTheater2);
            
            var actor1 = new Actor { Id = 1, Name = "Leonardo DiCaprio", Picture = "leo.jpg" };
            var actor2 = new Actor { Id = 2, Name = "Christian Bale", Picture = "bale.jpg" };
           
            _appContext.AddRange(actor1, actor2);


            var movies = new List<Movie>
    {
        new Movie
        {
            Id = 1,
            Title = "Inception 2",
            ReleaseDate = new DateTime(2024, 10, 10),
            MovieGenres = new List<MovieGenre>
            {
                new MovieGenre { GenreId = genre1.Id, Genre = genre1 }
            },
            CinemaScreenings = new List<CinemaScreening>
            {
                new CinemaScreening()
                {
                    MovieTheaterId = movieTheater1.Id,
                    MovieTheater = movieTheater1
                }
            },
            MovieActors = new List<MovieActor>
            {
                new MovieActor() { ActorId = actor1.Id, Actor = actor2, CharacterName = "Cobb"}
            }
        },
        new Movie
        {
            Id = 2,
            Title = "The Dark Knight",
            ReleaseDate = new DateTime(2024, 12, 25),
            MovieGenres = new List<MovieGenre>
            {
                new MovieGenre { GenreId = genre2.Id, Genre = genre2 }
            },
            CinemaScreenings = new List<CinemaScreening>
            {
                new CinemaScreening()
                {
                    MovieTheaterId = movieTheater2.Id,
                    MovieTheater = movieTheater2
                }
            },
            MovieActors =  new List<MovieActor>()
            {
                new MovieActor()
                {
                    ActorId = actor2.Id,
                    Actor = actor2,
                    CharacterName = "Bruce Wayne"
                }
            }
        }
    };
            
            _appContext.Movies.AddRange(movies);
            _appContext.SaveChanges();

            var movieGenre1 = new MovieGenre()
            {
                MovieId = 3,
                GenreId = 3
            };

            var movieGenre2 = new MovieGenre()
            {
                MovieId = 4,
                GenreId = 4
            };
            
            _appContext.MovieGenres.AddRange(movieGenre1, movieGenre2);
            _appContext.SaveChanges();
        }

        public void Dispose()
        {
            _appContext.Database.EnsureDeleted();
            _appContext.Dispose();
        }

        [Fact]
        public async Task Get_ReturnsMoviesList_WhenMoviesExist()
        {
            //Act
            var result = await _controller.Get();
            
            //Assert
            var actionResult = Assert.IsType<ActionResult<HomePageDTO>>(result);
            var actionResultVal = Assert.IsType<HomePageDTO>(actionResult.Value);
            Assert.NotNull(actionResultVal.UpcomingMovies);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_IfMovieDoesNotExist()
        {
            //Arrange
            int movieId = 99;

            //Act
            var result = await _controller.Get(movieId);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
        

        [Fact]
        public async Task Get_ById_ReturnsMovie_IfMovieExists()
        {
            //Arrange
            int movieId = 1;

            //Act
            var result = await _controller.Get(movieId);

            //Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<MovieDetailsDTO>>(result);
            var movieReturnVal = Assert.IsType<MovieDetailsDTO>(actionResult.Value);
            Assert.Equal("Inception 2", movieReturnVal.Title);
        }

        [Fact]
        public async Task PostGet_ReturnsGenreAndMovieTheaters()
        {
            //Act
            var result = await _controller.PostGet();

            //Assert
            var value = Assert.IsType<MoviesPostGetOptionsDTO>(result.Value);
            Assert.Equal(2, value.Genres.Count);
            Assert.Equal(2, value.MoviesTheaters.Count);
        }

        [Fact]
        public async Task Post_ReturnsCreatedAtRouteResult()
        {
            
            var posterUrl = "http://example.com/poster.jpg";
            _mockFileStorage.Setup(f => f.SaveFile(It.IsAny<string>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(posterUrl);
            
            var fakeMovieCreationDTO = GetMovieCreationDTO();

            //Act
            var result = await _controller.Post(fakeMovieCreationDTO);
            
            //Assert
            var createdAtRouteRes = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetMovieById", createdAtRouteRes.RouteName);
            Assert.Equal(3, ((MovieReadDTO)createdAtRouteRes.Value!).Id);
        }

        [Fact]
        public async Task Post_ReturnsCreatedAtRouteResult_WhenMovieHasNoPoster()
        {
            var fakeMovieCreationDTO = GetMovieCreationDTO();

            //Act
            var result = await _controller.Post(fakeMovieCreationDTO);

            //Assert
            var createdAtRouteRes = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetMovieById", createdAtRouteRes.RouteName);
            Assert.Equal(3, ((MovieReadDTO)createdAtRouteRes.Value!).Id);
        }
        
        [Fact]
        public async Task PutGet_ReturnsMovieGenreAndMovieTheaters()
        {
            //Arrange

            var movieId = 1;
            
            //Act
            var result = await _controller.PutGet(movieId);

            //Assert
            var value = Assert.IsType<MoviesPutGetOptionsDTO>(result.Value);
            Assert.Single(value.SelectedGenres);
            Assert.Equal("Inception 2", value.Movie.Title);
            Assert.Single(value.NonSelectedGenres);
            Assert.Single(value.SelectedTheaters);
            Assert.Single(value.NonSelectedTheaters);
            Assert.Single(value.Actors);

        }

        [Fact]
        public async Task Put_ReturnsNoContent_WhenMovieIsUpdated()
        {
            //Arrange

            var movieId = 1;

            var movieCreationDto = new MovieCreationDTO()
            {
                Title = "Inception 2: The Inceptioning",
                Poster = new FormFile(null, 0, 0, "Poster", "new-poster.jpg")
            };

            //Act
            var result = await _controller.Put(movieId, movieCreationDto);

            //Assert
            Assert.IsType<NoContentResult>(result);
            var movieSavedTitle = _appContext.Movies.Find(movieId)?.Title;
            Assert.Equal(movieCreationDto.Title, movieSavedTitle);
            _mockFileStorage.Verify(f => f.SaveEditedFile(It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<IFormFile>()), Times.Once);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default));
        }

        [Fact]
        public async Task Put_ReturnsNotFound_WhenMovieIsNotFound()
        {
            //Arrange

            var movieId = 99;

            var movieCreationDto = new MovieCreationDTO()
            {
                Title = "New Avengers",
            };
            
            //Act
            var result = await _controller.Put(movieId, movieCreationDto);
            
            //Assert
             Assert.IsType<NotFoundResult>(result);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task Delete_ShouldRemoveMovieAndReturnNoContent()
        {
            //Arrange
            int movieId = 1;
            
            //Act
            var result = await _controller.Delete(movieId);
            
            //Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _appContext.Movies.FindAsync(movieId));
            _mockFileStorage.Verify(f => f.Delete(It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(),
                default), Times.Once);

        }

        [Fact]
        public async Task Delete_MovieDoesNotExist_ReturnNotFound()
        {
            //Arrange
            var movieId = 99;
            
            //Act
            var result = await _controller.Delete(movieId);

            //Assert
            Assert.IsType<NotFoundResult>(result);
            
            _mockFileStorage.Verify(f => f.Delete(It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(),
                default), Times.Never);
        }
        
        private MovieCreationDTO GetMovieCreationDTO()
        {
            // Mocking IFormFile for the Poster property
            var posterMock = new Mock<IFormFile>();
            posterMock.Setup(_ => _.FileName).Returns("poster.jpg");
            posterMock.Setup(_ => _.Length).Returns(1024);

            return new MovieCreationDTO
            {
                Title = "The Epic Journey",
                Trailer = "https://www.youtube.com/watch?v=example",
                ReleaseDate = new DateTime(2024, 12, 25),
                Poster = posterMock.Object,
                GenresIds = new List<int> { 1, 2, 3 },
                MovieTheatersIds = new List<int> { 10, 20 },
                Actors = new List<MovieActorCreationDTO>
                {
                    new MovieActorCreationDTO { Id = 1, CharacterName = "Hero" },
                    new MovieActorCreationDTO { Id = 2, CharacterName = "Villain" }
                 }
            };
        }
        

    }
}
