using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_NET.Models
{
    public class Drone
    {
        [Key]
        public int DroneId { get; set; }

        [Required(ErrorMessage = "Please enter the drone callsign")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 100 characters")]
        public string CallSign { get; set; }

        [Required(ErrorMessage = "Enter drone status")]
        public DStatus Status { get; set; }

        [Required]
        public int[] CurrentCoords { get; set; } = new int[2];

        [NotMapped]
        public int CoordX
        {
            get => CurrentCoords.Length > 0 ? CurrentCoords[0] : 0;
            set => CurrentCoords[0] = value;
        }

        [NotMapped]
        public int CoordY
        {
            get => CurrentCoords.Length > 1 ? CurrentCoords[1] : 0;
            set => CurrentCoords[1] = value;
        }


        [ForeignKey("Model")]
        public int ModelId { get; set; }

        public Model? Model {  get; set; }

        [Required(ErrorMessage = "Please enter the drone range")]
        public int Range { get; set; }

        [ForeignKey("DroneCloud")]
        public int? DroneCloudId { get; set; }

        public DroneCloud? DroneCloud { get; set; }

        [Display(Name = "Aquisition Date: ")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime AqDate { get; set; }


    }
}
