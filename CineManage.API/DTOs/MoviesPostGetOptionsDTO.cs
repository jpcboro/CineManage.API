namespace CineManage.API.DTOs
{
    public class MoviesPostGetOptionsDTO
    {
        public List<GenreReadDTO> Genres { get; set; } = new List<GenreReadDTO>();
        public List<MovieTheaterReadDTO> MoviesTheaters { get; set; } = new List<MovieTheaterReadDTO>();
    }
}
