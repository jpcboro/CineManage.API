namespace CineManage.API.DTOs
{
    public class MovieReadDTO
    {
        public int Id { get; set; }
        public required string Title {  get; set; }
        public string? Trailer { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? Poster { get; set; }
        public double AverageRating { get; set; }
        public int UserRating { get; set; }
    }
}
