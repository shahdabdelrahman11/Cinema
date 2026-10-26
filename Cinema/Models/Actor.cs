
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; } 

        public string? Bio { get; set; }

        public string? Img { get; set; }
    }
}
