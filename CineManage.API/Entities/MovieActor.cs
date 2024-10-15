using System.ComponentModel.DataAnnotations;

namespace CineManage.API.Entities
{
    public class MovieActor
    {
        public int MovieId { get; set; }
        public int ActorId { get; set; }
        public int Order { get; set; }
        [StringLength(300)]
        public required string CharacterName { get; set; }
        public Movie Movie { get; set; } = null!;
        public Actor Actor { get; set; } = null!;
        
    }
}
