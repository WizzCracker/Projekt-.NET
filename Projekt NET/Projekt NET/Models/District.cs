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
        public List<Coordinate> BoundingPoints { get; set; } = new();

    }
}
