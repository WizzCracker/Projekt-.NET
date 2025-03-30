using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum DType
{
    [Display(Name = "Acquisition")]
    Acquisition = 1,

    [Display(Name = "Dropoff")]
    Dropoff = 2,
}

namespace Projekt_NET.Models
{
    public class Delivery
    {
        [Key]
        public int DeliveryId {  get; set; }

        [Required(ErrorMessage = "Enter delivery type")]
        public DType DeliveryType { get; set; }

        [ForeignKey("Package")]
        public int PackageId { get; set; }

        public Package Package { get; set; }

    }
}