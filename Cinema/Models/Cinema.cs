
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } 

        public string? Description { get; set; }

        public string? Logo { get; set; }

        public string? Address { get; set; }
    }
}
