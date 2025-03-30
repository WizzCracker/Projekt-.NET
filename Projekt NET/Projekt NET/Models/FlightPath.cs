using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class FlightPath
    {
        [Key]
        public int FlightPathId { get; set; }

        //Potrzebuje walidacji: loty musz¹ zgadzaæ siê czasowo, dron nie mo¿e mieæ przesy³ki przy locie odbiorczym (nie mo¿e byæ 2 lotów tego same typu po sobie)
        public List<Flight> FlightList { get; set; }
    }
}