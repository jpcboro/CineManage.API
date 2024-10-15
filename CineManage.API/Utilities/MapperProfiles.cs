using AutoMapper;
using CineManage.API.DTOs;
using CineManage.API.Entities;
using NetTopologySuite.Geometries;

namespace CineManage.API.Utilities
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            
        }
        public MapperProfiles(GeometryFactory geoFactory)
        {
            ConfigureGenres();
            ConfigureActors();
            ConfigureMovieTheaters(geoFactory);
            ConfigureMovies();
        }

        private void ConfigureMovies()
        {
            CreateMap<MovieCreationDTO, Movie>()
                .ForMember(movie => movie.Poster, options => options.Ignore())
                .ForMember(movie => movie.MovieGenres, options =>
                options.MapFrom(mCreationDTO => mCreationDTO.GenresIds!.Select(id => new MovieGenre { GenreId = id })))
                .ForMember(movie => movie.CinemaScreenings, options =>
                options.MapFrom(mCreationDTO => mCreationDTO.MovieTheatersIds!.Select(id => new CinemaScreening { MovieTheaterId = id })))
                .ForMember(movie => movie.MovieActors, options => options.MapFrom(mCreationDTO => mCreationDTO.Actors!.Select(actor =>
                new MovieActor { ActorId = actor.Id, CharacterName = actor.Character })));


            CreateMap<Movie, MovieReadDTO>();

            CreateMap<Movie, MovieDetailsDTO>()
                .ForMember(movieDetails => movieDetails.Genres, options => 
                    options.MapFrom(movie => movie.MovieGenres))
                .ForMember(movieDetails => movieDetails.MovieTheaters, options =>
                options.MapFrom(movie => movie.CinemaScreenings))
                .ForMember(movieDetails => movieDetails.Actors, options => 
                options.MapFrom(movie => movie.MovieActors.OrderBy(movieActor => movieActor.Order)));

            CreateMap<MovieGenre, GenreReadDTO>()
                .ForMember(genreDTO => genreDTO.Id, options =>
                options.MapFrom(movieGenre => movieGenre.GenreId))
                .ForMember(genreDTO => genreDTO.Name, options =>
                options.MapFrom(movieGenre => movieGenre.Genre.Name));

            CreateMap<CinemaScreening, MovieTheaterReadDTO>()
                .ForMember(mTheaterDTO => mTheaterDTO.Id, options =>
                    options.MapFrom(cinema => cinema.MovieTheaterId))
                .ForMember(mTheaterDTO => mTheaterDTO.Name, options =>
                options.MapFrom(cinema => cinema.MovieTheater.Name))
                .ForMember(mTheaterDTO => mTheaterDTO.Latitude, options =>
                options.MapFrom(cinema => cinema.MovieTheater.Location.Y))
                .ForMember(mTheaterDTO => mTheaterDTO.Longitude, options =>
                options.MapFrom(cinema => cinema.MovieTheater.Location.X));

            CreateMap<MovieActor, MovieActorReadDTO>()
                .ForMember(mActorDTO => mActorDTO.Id, options =>
                options.MapFrom(mActor => mActor.ActorId))
                .ForMember(mActorDTO => mActorDTO.Name, options =>
                options.MapFrom(mActor => mActor.Actor.Name))
                .ForMember(mActorDTO => mActorDTO.Picture, options =>
                options.MapFrom(mActor => mActor.Actor.Picture));


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
            CreateMap<Actor, MovieActorReadDTO>();
        }

        private void ConfigureGenres()
        {
            CreateMap<GenreCreationDTO, Genre>();
            CreateMap<Genre, GenreReadDTO>();
        }
    }
}
