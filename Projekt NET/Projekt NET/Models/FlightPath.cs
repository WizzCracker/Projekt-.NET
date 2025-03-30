using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class FlightPath
    {
        [Key]
        public int FlightPathId { get; set; }

        //Potrzebuje walidacji: loty musz� zgadza� si� czasowo, dron nie mo�e mie� przesy�ki przy locie odbiorczym (nie mo�e by� 2 lot�w tego same typu po sobie)
        public List<Flight> FlightList { get; set; }
    }
}