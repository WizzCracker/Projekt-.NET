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

        [NotMapped]
        public CancellationTokenSource? CancellationSource { get; set; }

        public async Task MoveToAsync(double targetLat, double targetLng, DroneDbContext context, CancellationToken token)
        {
            if (Model == null || Coordinate == null)
                throw new InvalidOperationException("Drone must have assigned model and coordinates.");

            double speedMps = Model.MaxSpeed / 3.6;
            double currentLat = Coordinate.Latitude;
            double currentLng = Coordinate.Longitude;

            double totalDistance = GeoFunctions.HaversineDistance(currentLat, currentLng, targetLat, targetLng) * 1000;
            int totalSteps = Math.Max((int)(totalDistance / speedMps), 1);

            Status = DStatus.Busy;

            var flight = context.Flights.FirstOrDefault(f =>
               f.DroneId == DroneId &&
               f.ArrivDate == null &&
               f.DeliveryCoordinates.Latitude == targetLat &&
               f.DeliveryCoordinates.Longitude == targetLng);

            if (flight == null)
                throw new InvalidOperationException("No active flight found for this drone.");

            flight.Steps = totalSteps;
            context.Update(flight);

            var stopwatch = new Stopwatch();

            for (int i = 1; i <= totalSteps; i++)
            {
                if (token.IsCancellationRequested)
                {
                    flight.ArrivDate = DateTime.UtcNow;
                    Status = DStatus.Active;

                    context.Update(this);
                    context.Update(flight);
                    await context.SaveChangesAsync();
                    return;
                }

                stopwatch.Restart();

                double progress = (double)i / totalSteps;
                Coordinate.Latitude = GeoFunctions.Lerp(currentLat, targetLat, progress);
                Coordinate.Longitude = GeoFunctions.Lerp(currentLng, targetLng, progress);

                flight.Steps = totalSteps - i;

                context.Update(this);
                context.Update(flight);
                await context.SaveChangesAsync();

                int delay = Math.Max(1000 - (int)stopwatch.ElapsedMilliseconds, 0);
                await Task.Delay(delay, token);
            }

            flight.ArrivDate = DateTime.UtcNow;
            Status = DStatus.Active;

            context.Update(this);
            context.Update(flight);
            await context.SaveChangesAsync();
        }

        

        public void StopMovement(DroneDbContext context)
        {
            CancellationSource?.Cancel();
        }
    }
}
