﻿using System.ComponentModel.DataAnnotations;
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
        public double? Weight { get; set; }
        [Required(ErrorMessage = "Enter delivery status of package.")]
        public string Status { get; set; }
        [Required]
        public string TargetAddress { get; set; }
    }
}
