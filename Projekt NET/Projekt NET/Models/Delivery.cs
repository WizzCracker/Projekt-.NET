using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class Delivery
    {
        [Key]
        public int DeliveryId {  get; set; }

        [Required(ErrorMessage = "Enter delivery type")]
        public DType Type { get; set; }

        [ForeignKey("Package")]
        public int PackageId { get; set; }

        public Package? Package { get; set; }

    }
}