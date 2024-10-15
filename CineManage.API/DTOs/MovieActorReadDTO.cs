namespace CineManage.API.DTOs
{
    public class MovieActorReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Picture { get; set; }
        public string CharacterName { get; set; } = null!;
    }
}
