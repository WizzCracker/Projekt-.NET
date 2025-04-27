using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class Package
    {
        [Key]
        public int PackageId { get; set; }
        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public Client Client { get; set; }
        [ForeignKey("Drone")]
        public int? DroneId { get; set; }
        public Drone Drone { get; set; }
        public double? Weight { get; set; }
        [Required]
        public string TargetAddress { get; set; }
    }
}
