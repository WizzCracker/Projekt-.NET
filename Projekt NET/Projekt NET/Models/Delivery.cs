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
        public enum DelivType
        {
            [Display(Name = "Acquisition")]
            Acquisition = 1,

            [Display(Name = "Dropoff")]
            Dropoff = 2,
        }

        [ForeignKey("Package")]
        public int PackageId { get; set; }

        public Package Package { get; set; }

    }
}