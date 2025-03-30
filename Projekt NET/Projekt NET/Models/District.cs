using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    public class District
    {
        [Key]
        public int DistrictId { get; set; }

        [Required(ErrorMessage = "Enter district name")]
        public string Name { get; set;}

        [Required]
        public float[] BoundingPoints { get; set; }

        [Required]
        public List<DroneCloud> DroneClouds { get; set; }

        [Required]
        public List<Client> Clients { get; set; }

    }
}
