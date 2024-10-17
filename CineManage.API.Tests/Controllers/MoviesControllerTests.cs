using AutoFixture;
using AutoMapper;
using CineManage.API.Controllers;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using CineManage.API.Services;
using CineManage.API.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineManage.API.Tests.Controllers
{
    public class MoviesControllerTests
    {
        private readonly Mock<IOutputCacheStore> _mockOutputCacheStore;
        private readonly ApplicationContext _appContext;
        private readonly Mock<IMapper> _mockMapper;
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

            _mockMapper = new Mock<IMapper> ();

            _mockFileStorage = new Mock<IFileStorage>();

            _controller = new MoviesController(_appContext, _mockMapper.Object,
                _mockOutputCacheStore.Object, _mockFileStorage.Object);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private void SeedData()
        {
            // Ensure genres are only created once and reused.
            var genre1 = new Genre { Id = 1, Name = "Science Fiction" };
            var genre2 = new Genre { Id = 2, Name = "Action" };

            _appContext.Genres.AddRange(genre1, genre2);
            _appContext.SaveChanges();

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
            _appContext.SaveChanges();

            var movies = new List<Movie>
    {
        new Movie
        {
            Id = 1,
            Title = "Inception",
            ReleaseDate = new DateTime(2010, 7, 16),
            MovieGenres = new List<MovieGenre>
            {
                new MovieGenre { GenreId = genre1.Id, Genre = genre1 }
            }
        },
        new Movie
        {
            Id = 2,
            Title = "The Dark Knight",
            ReleaseDate = new DateTime(2008, 7, 18),
            MovieGenres = new List<MovieGenre>
            {
                new MovieGenre { GenreId = genre2.Id, Genre = genre2 }
            }
        }
    };

            // Add the movies to the context.
            _appContext.Movies.AddRange(movies);
            _appContext.SaveChanges();
        }

        public void Dispose()
        {
            _appContext.Database.EnsureDeleted();
            _appContext.Dispose();
        }

        [Fact]
        public async Task Get_ReturnsNotFound_IfMovieDoesNotExist()
        {
            //Arrange
            int movieId = 99;

            _mockMapper.Setup(m => m.ConfigurationProvider)
                .Returns(new MapperConfiguration(m => m.CreateMap<Movie, MovieDetailsDTO>()));

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
            _mockMapper.Setup(m => m.ConfigurationProvider)
                .Returns(new MapperConfiguration(m => m.CreateMap<Movie, MovieDetailsDTO>()));

            //Act
            var result = await _controller.Get(movieId);

            //Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<MovieDetailsDTO>>(result);
            var movieReturnVal = Assert.IsType<MovieDetailsDTO>(actionResult.Value);
            Assert.Equal("Inception", movieReturnVal.Title);
        }

        [Fact]
        public async Task PostGet_ReturnsGenreAndMovieTheaters()
        {
            //Arrange

            var config = new MapperConfiguration(confg =>
            {
                confg.CreateMap<Genre, GenreReadDTO>();
                confg.CreateMap<MovieTheater, MovieTheaterReadDTO>();
            });
            _mockMapper.Setup(m => m.ConfigurationProvider).Returns(config);
           
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
            //Arrange
            _mockMapper.Setup(m => m.Map<Movie>(It.IsAny<MovieCreationDTO>()))
                .Returns(GetSampleMovie());
                
            var posterUrl = "http://example.com/poster.jpg";
            _mockFileStorage.Setup(f => f.SaveFile(It.IsAny<string>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(posterUrl);

            _mockMapper.Setup(m => m.Map<MovieReadDTO>(It.IsAny<Movie>()))
                .Returns(GetMovieReadDTO());

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
            //Arrange
            _mockMapper.Setup(m => m.Map<Movie>(It.IsAny<MovieCreationDTO>()))
                .Returns(GetSampleMovie());

            _mockMapper.Setup(m => m.Map<MovieReadDTO>(It.IsAny<Movie>()))
                .Returns(GetMovieReadDTO());

            var fakeMovieCreationDTO = GetMovieCreationDTO();

            //Act
            var result = await _controller.Post(fakeMovieCreationDTO);


            //Assert
            var createdAtRouteRes = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetMovieById", createdAtRouteRes.RouteName);
            Assert.Equal(3, ((MovieReadDTO)createdAtRouteRes.Value!).Id);
        }

        private MovieReadDTO GetMovieReadDTO()
        {
            return new MovieReadDTO
            {
                Id = 3,
                Title = "The Epic Journey",
                Trailer = "https://www.youtube.com/watch?v=example",
                ReleaseDate = new DateTime(2024, 12, 25),
                Poster = "poster.jpg"
            };
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
                    new MovieActorCreationDTO { Id = 1, Character = "Hero" },
                    new MovieActorCreationDTO { Id = 2, Character = "Villain" }
                 }
            };
        }

        public Movie GetSampleMovie()
        {

           
            var movie = new Movie
            {
                Id = 3,
                Title = "The Epic Journey",
                Trailer = "https://www.youtube.com/watch?v=example",
                ReleaseDate = new DateTime(2024, 12, 25),
                Poster = "poster.jpg",
                MovieGenres = new List<MovieGenre>
            {
                new MovieGenre { GenreId = 3, Genre = new Genre { Id = 3, Name = "Adventure" } },
                new MovieGenre { GenreId = 4, Genre = new Genre { Id = 4, Name = "Fantasy" } },
                new MovieGenre { GenreId = 5, Genre = new Genre { Id = 5, Name = "Action" } }
            },
                CinemaScreenings = new List<CinemaScreening>
            {
                new CinemaScreening
                {
                    MovieId = 3,
                    MovieTheaterId = 10,
                    Movie = null!,
                    MovieTheater =  new MovieTheater
                    {
                        Id = 10,
                        Name = "Carlton Indie",
                        Location = new Point(-37.805508, 144.971445)
                        {
                            SRID = 4326
                        }
                    }
                },
                new CinemaScreening
                {
                    MovieId = 3,
                    MovieTheaterId = 20,
                    Movie = null!,
                    MovieTheater = new MovieTheater
                    {
                        Id = 20,
                        Name = "Royal Botanical Gardens Theatre",
                        Location = new Point(-37.828806, 144.980058)
                        {
                            SRID = 4326
                        }
                    }
                }
            },
                MovieActors = new List<MovieActor>
            {
                new MovieActor
                {
                    ActorId = 1,
                    MovieId = 1,
                    CharacterName = "Hero",
                    Order = 1,
                    Movie = null!,
                    Actor = new Actor { Id = 1, Name = "John Doe" }
                },
                new MovieActor
                {
                    ActorId = 2,
                    MovieId = 1,
                    CharacterName = "Villain",
                    Order = 2,
                    Movie = null!,
                    Actor = new Actor { Id = 2, Name = "Jane Smith" }
                }
            }
            };

            foreach (var screening in movie.CinemaScreenings)
            {
                screening.Movie = movie;
            }

            foreach (var movieActor in movie.MovieActors)
            {
                movieActor.Movie = movie;
            }

            return movie;
        }

    }
}
