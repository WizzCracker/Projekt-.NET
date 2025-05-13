using Projekt_NET.Models.System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

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
            Console.WriteLine(totalSteps);
            if (totalSteps < 1) totalSteps = 1;
            Status = DStatus.Busy;
            var stopwatch = new Stopwatch();
            Console.WriteLine(totalSteps);
            var flight = context.Flights.FirstOrDefault(f => f.DroneId == DroneId && f.ArrivDate == null && f.DeliveryCoordinates.Latitude == targetLat && f.DeliveryCoordinates.Longitude == targetLng);

            if (flight == null)
            {
                throw new InvalidOperationException("No active flight found for this drone.");
            }
            Console.WriteLine(totalSteps);
            flight.Steps = totalSteps;
            Console.WriteLine(totalSteps);
            for (int i = 1; i <= totalSteps; i++)
            {
                stopwatch.Restart();
                double progress = (double)i / totalSteps;
                Console.WriteLine(totalSteps);
                double newLat = GeoFunctions.Lerp(currentLat, targetLat, progress);
                double newLng = GeoFunctions.Lerp(currentLng, targetLng, progress);

                Coordinate.Latitude = newLat;
                Coordinate.Longitude = newLng;

                flight.Steps = totalSteps - i;

                context.Update(this);
                await context.SaveChangesAsync();
                Console.WriteLine((int)stopwatch.ElapsedMilliseconds);
                await Task.Delay(1000 - (int)stopwatch.ElapsedMilliseconds);
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }
            Console.WriteLine(totalSteps);
            flight.ArrivDate = DateTime.UtcNow;

            Status = DStatus.Active;
            context.Update(this);
            await context.SaveChangesAsync();
        }
    }
}
