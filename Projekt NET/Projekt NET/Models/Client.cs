﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Enter client name.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Enter client surname.")]
        public string Surname { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? PhoneNumber { get; set; }

        public List<Package> Packages { get; set; } = new();

        [ForeignKey("District")]
        public int DistrictId { get; set; }

        public District District { get; set; }
    }
}
