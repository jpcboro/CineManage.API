namespace CineManage.API.DTOs;

public class HomePageDTO
{
    public List<MovieReadDTO> NowShowing { get; set; } = new List<MovieReadDTO>();
    public List<MovieReadDTO> UpcomingMovies { get; set; } = new List<MovieReadDTO>();
}