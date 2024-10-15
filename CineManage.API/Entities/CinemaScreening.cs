namespace CineManage.API.Entities
{
    public class CinemaScreening
    {
        public int MovieId { get; set; }
        public int MovieTheaterId { get; set; }
        public Movie Movie { get; set; } = null!;
        public MovieTheater MovieTheater { get; set; } = null!;
    }
}
