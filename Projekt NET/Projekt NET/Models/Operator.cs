using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    public class Operator
    {
        [Key]
        public int OperatorId { get; set; }

        [Required(ErrorMessage = "Enter operator name.")]
        public string Name { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? PhoneNumber { get; set; }

        public List<DroneCloud> ManagedDroneClouds { get; set; }
    }

}
