using Projekt_NET.Models.System;
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

        public Coordinate Coordinate { get; set; }

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


        public async Task MoveToAsync(double targetLat, double targetLng, DroneDbContext context)
        {
            if (Model == null || Coordinate == null)
                throw new InvalidOperationException("Drone must have assigned model and coordinates.");

            double speedMps = Model.MaxSpeed / 3.6;

            double currentLat = Coordinate.Latitude;
            double currentLng = Coordinate.Longitude;

            double totalDistance = GeoFunctions.HaversineDistance(currentLat, currentLng, targetLat, targetLng) * 1000; // meters
            int totalSteps = (int)(totalDistance / speedMps);
            if (totalSteps < 1) totalSteps = 1;

            for (int i = 1; i <= totalSteps; i++)
            {
                double progress = (double)i / totalSteps;

                double newLat = GeoFunctions.Lerp(currentLat, targetLat, progress);
                double newLng = GeoFunctions.Lerp(currentLng, targetLng, progress);

                Coordinate.Latitude = newLat;
                Coordinate.Longitude = newLng;

                context.Update(this);
                await context.SaveChangesAsync();

                await Task.Delay(1000);
            }
            Status = DStatus.Active;
            context.Update(this);
            await context.SaveChangesAsync();
        }
    }
}
