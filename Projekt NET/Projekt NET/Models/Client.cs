﻿using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    public class Client
    {
        [Key]
        public int clientId { get; set; }

        [Required(ErrorMessage = "Enter client name.")]
        public string name { get; set; }

        [Required(ErrorMessage = "Enter client surname.")]
        public string surname { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? phoneNumber { get; set; }

        public List<Package> packages { get; set; } = new();
    }
}
