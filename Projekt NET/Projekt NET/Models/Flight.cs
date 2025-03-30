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
        public DateTime DepDate { get; set; }

        [Display(Name = "Planned arrival date: ")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ArrivDate { get; set; }

        [ForeignKey("Delivery")]
        public int DeliveryId { get; set; }

        public Delivery Delivery { get; set; }

        [Required]
        public float DeliveryLat { get; set; }

        [Required]
        public float DeliveryLng { get; set; }
    }
}