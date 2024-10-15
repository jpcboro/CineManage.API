namespace CineManage.API.DTOs
{
    public class MovieDetailsDTO : MovieReadDTO
    {
       public List<GenreReadDTO> Genres {  get; set; } = new List<GenreReadDTO>();
       public List<MovieTheaterReadDTO> MovieTheaters { get; set; }= new List<MovieTheaterReadDTO>();
       public List<MovieActorReadDTO> Actors { get; set; } = new List<MovieActorReadDTO>();
    }
}
