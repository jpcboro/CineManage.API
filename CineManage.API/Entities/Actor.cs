using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CineManage.API.Entities
{
    public class Actor
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public required string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        [Unicode(false)]
        public string? Picture { get; set; }
    }
}
