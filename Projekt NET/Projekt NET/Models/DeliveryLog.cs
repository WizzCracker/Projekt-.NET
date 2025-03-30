using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    public class DeliveryLog
    {
        [Key]
        public int DeliveryLogId { get; set; }

        [ForeignKey("Package")]
        public int PackageId { get; set; }

        public Package Package { get; set; }

        public DateTime LogDate { get; set; }

        [Required]
        public Status status { get; set; }

        public string Remarks { get; set; }
    }

}
