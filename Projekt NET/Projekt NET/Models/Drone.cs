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

            const double EarthRadius = 6371000; // meters
            double speedMps = Model.MaxSpeed / 3.6; // km/h -> m/s

            double currentLat = Coordinate.Latitude;
            double currentLng = Coordinate.Longitude;

            double totalDistance = HaversineDistance(currentLat, currentLng, targetLat, targetLng) * 1000; // meters
            int totalSteps = (int)(totalDistance / speedMps);
            if (totalSteps < 1) totalSteps = 1;

            for (int i = 1; i <= totalSteps; i++)
            {
                double progress = (double)i / totalSteps;

                double newLat = Lerp(currentLat, targetLat, progress);
                double newLng = Lerp(currentLng, targetLng, progress);

                Coordinate.Latitude = newLat;
                Coordinate.Longitude = newLng;

                context.Update(this);
                await context.SaveChangesAsync();

                await Task.Delay(1000); // wait 1 second per step
            }
        }

        private double Lerp(double start, double end, double t)
        {
            return start + (end - start) * t;
        }

        private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double angle)
        {
            return angle * Math.PI / 180;
        }
    }
}
