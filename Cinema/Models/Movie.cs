namespace Cinema.Models
{
    public class Movie
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Des { get; set; }

        public decimal Price { get; set; }

        public bool Status { get; set; }

        public DateTime DateTime { get; set; }

        public string? MainImg { get; set; }

        public List<MovieSubImage> SubImages { get; set; } = new();

        public List<Actor> Actors { get; set; } = new();

        public int CategoryId { get; set; }

        public int CinemaId { get; set; }

        public Category? Category { get; set; }

        public global::Cinema.Models.Cinema? Cinema { get; set; }
    }
}