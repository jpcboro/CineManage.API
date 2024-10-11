using AutoMapper;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using NetTopologySuite.Geometries;

namespace CineManage.API.Utilities
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles(GeometryFactory geoFactory)
        {
            ConfigureGenres();
            ConfigureActors();
            ConfigureMovieTheaters(geoFactory);
        }

        private void ConfigureMovieTheaters(GeometryFactory geometryFactory)
        {
            CreateMap<MovieTheaterCreationDTO, MovieTheater>()
                .ForMember(x => x.Location, x => x.MapFrom(
                            p => geometryFactory.CreatePoint(new Coordinate(p.Longitude, p.Latitude))));
           
            CreateMap<MovieTheater, MovieTheaterReadDTO>()
                .ForMember(x => x.Latitude, x => x.MapFrom(p => p.Location.Y))
                .ForMember(x => x.Longitude, x => x.MapFrom(p => p.Location.X));

        }

        private void ConfigureActors()
        {
            CreateMap<ActorCreationDTO, Actor>()
                .ForMember(a => a.Picture, options => options.Ignore());
            CreateMap<Actor, ActorReadDTO>();
        }

        private void ConfigureGenres()
        {
            CreateMap<GenreCreationDTO, Genre>();
            CreateMap<Genre, GenreReadDTO>();
        }
    }
}
