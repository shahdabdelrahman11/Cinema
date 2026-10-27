
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Models
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

    
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public int MovieId { get; set; }

        [ForeignKey(nameof(MovieId))]
        public Movie? Movie { get; set; }

      
        public int NumberOfTickets { get; set; } = 1;

       
        public DateTime ShowDateTime { get; set; }

        public DateTime BookingDate { get; set; } 

        public string PaymentMethod { get; set; } 
    }
}
