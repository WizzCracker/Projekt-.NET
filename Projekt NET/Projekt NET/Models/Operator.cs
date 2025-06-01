using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [ForeignKey("Drone")]
        public int? DroneId { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }
    }

}
