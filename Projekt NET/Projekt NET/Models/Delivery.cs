using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class Delivery
    {
        [Key]
        public int DeliveryId {  get; set; }

        [ForeignKey("Package")]
        public int PackageId { get; set; }

        public Package? Package { get; set; }

        [ForeignKey("FlightPath")]
        public int FlightPathId { get; set; }
        public FlightPath? FlightPath { get; set; }

    }
}