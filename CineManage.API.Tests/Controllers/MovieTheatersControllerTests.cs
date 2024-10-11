using AutoFixture;
using AutoMapper;
using CineManage.API.Controllers;
using CineManage.API.Data;
using CineManage.API.DTOs;
using CineManage.API.Entities;
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
    public class MovieTheatersControllerTests
    {
        private readonly Mock<IOutputCacheStore> _mockOutputCacheStore;
        private readonly ApplicationContext _appContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly MovieTheatersController _controller;

        public MovieTheatersControllerTests()
        {
            _mockOutputCacheStore = new Mock<IOutputCacheStore>();

            var options = new DbContextOptionsBuilder<ApplicationContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _appContext = new ApplicationContext(options);

            var fakeMovieTheaters = new List<MovieTheater>
{
    new MovieTheater
    {
        Id = 1,
        Name = "Carlton Indie",
        Location = new Point(-37.805508, 144.971445) // Coordinates for New York City
        {
            SRID = 4326 // Spatial reference system identifier for WGS84 (longitude, latitude)
        }
    },
    new MovieTheater
    {
        Id = 2,
        Name = "Royal Botanical Gardens Theatre",
        Location = new Point(-37.828806, 144.980058) // Coordinates for Los Angeles
        {
            SRID = 4326
        }
    },
    new MovieTheater
    {
        Id = 3,
        Name = "St Kilda Showing",
        Location = new Point(-37.86629, 144.97306) // Coordinates for San Francisco
        {
            SRID = 4326
        }
    }
};
            _appContext.MovieTheaters.AddRange(fakeMovieTheaters);
            _appContext.SaveChanges();

            _mockMapper = new Mock<IMapper>();

            _controller = new MovieTheatersController(appContext: _appContext,
                mapper: _mockMapper.Object,
                outputCacheStore: _mockOutputCacheStore.Object);

            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        public void Dispose()
        {
            _appContext.Database.EnsureDeleted();
            _appContext.Dispose();
        }

        [Fact]
        public async Task Get_ShouldReturnMovieTheatersList_WhenMovieTheatersExist()
        {
            //Arrange
            var pagination = new PaginationDTO()
            {
                PageNumber = 1,
                RecordsPerPage = 10
            };

            var fixture = new Fixture();
   
            var mTheaterReadDTOs = fixture.CreateMany<MovieTheaterReadDTO>();

            _mockMapper.Setup(m => m.ConfigurationProvider)
                .Returns(new MapperConfiguration(g => g.CreateMap<MovieTheater, MovieTheaterReadDTO>()));

            //Act
            var result = await _controller.Get(pagination);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Carlton Indie", result[0].Name);
        }

        [Fact]
        public async Task Post_ShouldCreateMovieTheaterAndReturnCreatedAtRouteWithMovieTheater()
        {
            //Arrange

            var mTheaterCreationDTO = new MovieTheaterCreationDTO 
            { 
                Name = "New Movie Theater",
                Latitude = -37.82464,
                Longitude = 144.97388
            };

            var mTheater = new MovieTheater 
            { 
                Id = 5, 
                Name = "New Movie Theater",
                Location = new Point(-37.82464, 144.97388)
                {
                    SRID = 4326 // Spatial reference system identifier for WGS84 (longitude, latitude)
                }
            };
            var mTheaterReadDTO = new MovieTheaterReadDTO 
            { Id = 5,
                Name = "New Movie Theater",
                Latitude = -37.82464,
                Longitude = 144.97388
            };

            _mockMapper.Setup(m => m.Map<MovieTheater>(mTheaterCreationDTO)).Returns(mTheater);

            _mockMapper.Setup(m => m.Map<MovieTheaterReadDTO>(mTheater)).Returns(mTheaterReadDTO);

            //Act
            var result = await _controller.Post(mTheaterCreationDTO);

            //Assert
            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetMovieTheaterById", createdAtRouteResult.RouteName);
            Assert.Equal("New Movie Theater", ((MovieTheaterReadDTO)createdAtRouteResult.Value!).Name);
            Assert.Equal(5, ((MovieTheaterReadDTO)createdAtRouteResult.Value).Id);

            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async Task Put_ShouldUpdateMovieTheaterAndReturnNoContent()
        {
            //Arrange
        
            var mCreationDTO = new MovieTheaterCreationDTO {
                Name = "Carlton Indie EDITED",
                Latitude = -73.935242,
                Longitude = 40.730610
            };


            var mtId = 1;
            var movieTheater = _appContext.MovieTheaters.Find(mtId);
            _appContext.Entry(movieTheater).State = EntityState.Detached;

            _mockMapper.Setup(m => m.Map<MovieTheater>(mCreationDTO))
                .Returns(
                new MovieTheater()
                {
                    Id = mtId,
                    Name = mCreationDTO.Name,
                    Location = new Point(mCreationDTO.Latitude, mCreationDTO.Longitude)
                }
                );                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      

            //Act
            var result = await _controller.Put(mtId, mCreationDTO);

            //Assert
            var updatedMovieTheater = await _appContext.MovieTheaters.FindAsync(mtId);
            Assert.Equal(mCreationDTO.Name, updatedMovieTheater?.Name);
            Assert.Equal(mCreationDTO.Latitude, updatedMovieTheater?.Location.X);
            Assert.Equal(mCreationDTO.Longitude, updatedMovieTheater?.Location.Y);
            Assert.IsType<NoContentResult>(result);
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);

        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenMovieTheaterDoesNotExist()
        {
            //Arrange
            var mtCreationDTO = new MovieTheaterCreationDTO
            {
                Name = "Carlton Indie EDITED",
                Latitude = -73.935242,
                Longitude = 40.730610
            };

            //Act
            var result = await _controller.Put(10, mtCreationDTO);

            //Assert

            Assert.IsType<NotFoundResult>(result);

            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Never);

        }

        [Fact]
        public async Task Delete_ShouldRemoveMovieTheaterAndReturnNoContent()
        {
            //Act
            int movieTheaterId = 1;
            var result = await _controller.Delete(movieTheaterId);

            //Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _appContext.MovieTheaters.FindAsync(movieTheaterId));
            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Once);

        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNoRecordIsDeleted()
        {
            //Act
            var result = await _controller.Delete(10);

            //Assert
            Assert.IsType<NotFoundResult>(result);

            _mockOutputCacheStore.Verify(o => o.EvictByTagAsync(It.IsAny<string>(), default), Times.Never);
        }


    }
}
