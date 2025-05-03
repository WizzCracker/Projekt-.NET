using System;
using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }

        [Display(Name = "Departure date: ")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DepDate { get; set; }

        [Display(Name = "Planned arrival date: ")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ArrivDate { get; set; }

        [Required]
        public List<Coordinate> DeliveryCoordinates { get; set; } = new();
    }
}