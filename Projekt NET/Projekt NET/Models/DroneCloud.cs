using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Projekt_NET.Models
{
    public class DroneCloud
    {
        [Key]
        public int DroneCloudId {  get; set; }

        public string Name { get; set; }

        [ForeignKey("District")]
        [Required(ErrorMessage = "Please assign the district of operations for the drone cloud")]
        public int DistrictId { get; set; }

        public District? District { get; set; }
    }
}
