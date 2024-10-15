namespace CineManage.API.DTOs
{
    public class MoviesPutGetOptionsDTO
    {
        public MovieReadDTO Movie { get; set; } = null!;
        public List<GenreReadDTO> SelectedGenres { get; set; } =  new List<GenreReadDTO>();
        public List<GenreReadDTO> NonSelectedGenres { get; set; } = new List<GenreReadDTO>();
        public List<MovieTheaterReadDTO> SelectedTheaters { get; set; } = new List<MovieTheaterReadDTO>();
        public List<MovieTheaterReadDTO> NonSelectedTheaters { get; set; } = new List<MovieTheaterReadDTO>();

        public List<MovieActorReadDTO> Actors { get; set;} = new List<MovieActorReadDTO>();

    }
}
