using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class FlightPath
    {
        [Key]
        public int FlightPathId { get; set; }

        public List<Flight> FlightList { get; set; } = new();

    }
}