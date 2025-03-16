using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class Package
    {
        [Key]
        public int packageId { get; set; }
        [ForeignKey("Client")]
        public int clientId { get; set; }
        public Client client { get; set; }
        public double? weight { get; set; }
        [Required(ErrorMessage = "Enter delivery status of package.")]
        public string status { get; set; }
        [Required]
        public string targetAddress { get; set; }
    }
}
