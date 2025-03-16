using System.ComponentModel.DataAnnotations;

namespace Projekt_NET.Models
{
    public class Model
    {
        [Key]
        public int ModelId { get; set; }

        [Required(ErrorMessage = "Enter model name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Enter max carry capacity")]
        public int MaxCapacity { get; set; }

        [Required(ErrorMessage = "Enter max speed")]
        public int MaxSpeed { get; set; }

        public string SpecialGear { get; set; } //Zależny od modelu specjalny sprzęt
    }
}
