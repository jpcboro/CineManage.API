namespace CineManage.API.DTOs
{
    public class ActorReadDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Picture { get; set; }
    }
}
