using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    public class DeliveryLog
    {
        [Key]
        public int DeliveryLogId { get; set; }

        [ForeignKey("Delivery")]
        public int DeliveryId { get; set; }

        public Delivery Delivery { get; set; }

        public DateTime LogDate { get; set; }

        public string Remarks { get; set; }
    }

}
