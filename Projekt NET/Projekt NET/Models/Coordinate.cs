using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    [Owned]
    public class Coordinate
    {
        [Key] 
        public int Id { get; set; }  
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
