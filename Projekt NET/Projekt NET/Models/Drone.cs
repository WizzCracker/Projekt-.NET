using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum DStatus
{
    [Display(Name = "Active")]
    Active = 1,

    [Display(Name = "Offline")]
    Offline = 2,

    [Display(Name = "Broken")]
    Broken = 3
}

namespace Projekt_NET.Models
{
    public class Drone
    {
        [Key]
        public int DroneId { get; set; }

        [Required(ErrorMessage = "Please enter the drone callsign")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 100 characters")]
        public string CallSign { get; set; }

        [Required(ErrorMessage = "Enter drone status")]
        public DStatus DroneStatus { get; set; }

        [Required]
        public float CurrentLat { get; set; }

        [Required]
        public float CurrentLng { get; set; }

        [ForeignKey("Model")]
        [Required(ErrorMessage = "Please enter the drone model")]
        public int ModelId { get; set; }

        public Model Model {  get; set; }

        [Required(ErrorMessage = "Please enter the drone range")]
        public int Range { get; set; }

        [ForeignKey("DroneCloud")]
        public int DroneCloudId { get; set; }

        public DroneCloud DroneCloud { get; set; }

        [Display(Name = "Aquisition Date: ")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime AqDate { get; set; }

        [ForeignKey("FlightPath")]
        public int FlightPathId { get; set; }

        public FlightPath FlightPath { get; set; }

    }
}
