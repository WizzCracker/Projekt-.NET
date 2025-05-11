using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }

        [Display(Name = "Departure date: ")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DepDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Arrival date: ")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ArrivDate { get; set; } = null;

        [ForeignKey("Drone")]
        public int? DroneId { get; set; }

        public Drone? Drone { get; set; }

        [Required]
        public Coordinate DeliveryCoordinates { get; set; } = new();
    }
}