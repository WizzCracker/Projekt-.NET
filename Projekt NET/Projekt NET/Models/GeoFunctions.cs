namespace Projekt_NET.Models
{
    public static class GeoFunctions
    {
        public static double Lerp(double start, double end, double t)
        {
            return start + (end - start) * t;
        }

        public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
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

        public static double ToRadians(double angle)
        {
            return angle * Math.PI / 180;
        }

        public static bool IsPointInDistrict(List<Coordinate> district, Coordinate point)
        {
            int count = district.Count;
            bool isInside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                double xi = district[i].Longitude;
                double yi = district[i].Latitude;
                double xj = district[j].Longitude;
                double yj = district[j].Latitude;

                bool intersects = ((yi > point.Latitude) != (yj > point.Latitude)) &&
                    (point.Longitude < (xj - xi) * (point.Latitude - yi) / (yj - yi + double.Epsilon) + xi);

                if (intersects)
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }

        public static Coordinate? FindIntersectionWithDistrictEdge(List<Coordinate> district, Coordinate start, Coordinate end)
        {
            int count = district.Count;

            for (int i = 0; i < count; i++)
            {
                var p1 = district[i];
                var p2 = district[(i + 1) % count];

                if (LineSegmentsIntersect(start, end, p1, p2, out var intersection))
                    return intersection;
            }

            return null;
        }



        public static bool LineSegmentsIntersect(Coordinate p, Coordinate p2, Coordinate q, Coordinate q2, out Coordinate intersection)
        {
            intersection = new Coordinate();

            double dx1 = p2.Longitude - p.Longitude;
            double dy1 = p2.Latitude - p.Latitude;

            double dx2 = q2.Longitude - q.Longitude;
            double dy2 = q2.Latitude - q.Latitude;

            double denominator = dx1 * dy2 - dy1 * dx2;

            const double epsilon = 1e-9;

            if (Math.Abs(denominator) < epsilon)
                return false;

            double dx = q.Longitude - p.Longitude;
            double dy = q.Latitude - p.Latitude;

            double t = (dx * dy2 - dy * dx2) / denominator;
            double u = (dx * dy1 - dy * dx1) / denominator;

            if (t >= -epsilon && t <= 1 + epsilon && u >= -epsilon && u <= 1 + epsilon)
            {
                // Przyciągnięcie wartości t i u do zakresu [0,1], jeśli są minimalnie poza nim
                t = Math.Clamp(t, 0, 1);
                u = Math.Clamp(u, 0, 1);

                intersection = new Coordinate
                {
                    Longitude = p.Longitude + t * dx1,
                    Latitude = p.Latitude + t * dy1
                };
                return true;
            }

            return false;
        }


        public static Coordinate MovePointTowards(Coordinate from, Coordinate to, double meters)
        {
            const double EarthRadius = 6371000; // metry
            var bearing = Math.Atan2(
                to.Longitude - from.Longitude,
                to.Latitude - from.Latitude
            );

            double delta = meters / EarthRadius;

            var lat = from.Latitude + delta * Math.Cos(bearing) * (180 / Math.PI);
            var lng = from.Longitude + delta * Math.Sin(bearing) * (180 / Math.PI) / Math.Cos(from.Latitude * Math.PI / 180);

            return new Coordinate
            {
                Latitude = lat,
                Longitude = lng
            };
        }


        public static Coordinate CalculateCentroid(List<Coordinate> points)
        {
            double lat = 0, lng = 0;
            foreach (var pt in points)
            {
                lat += pt.Latitude;
                lng += pt.Longitude;
            }

            return new Coordinate
            {
                Latitude = lat / points.Count,
                Longitude = lng / points.Count
            };
        }
        public static double DistanceToDistrictBoundary(List<Coordinate> district, Coordinate point)
        {
            double minDistance = double.MaxValue;

            for (int i = 0; i < district.Count; i++)
            {
                var p1 = district[i];
                var p2 = district[(i + 1) % district.Count];

                double dist = DistancePointToSegment(point, p1, p2);
                if (dist < minDistance)
                    minDistance = dist;
            }

            return minDistance;
        }

        private static double DistancePointToSegment(Coordinate p, Coordinate v, Coordinate w)
        {
            // Przekształcenie do wektora na płaszczyźnie
            double lat1 = v.Latitude;
            double lon1 = v.Longitude;
            double lat2 = w.Latitude;
            double lon2 = w.Longitude;

            double latP = p.Latitude;
            double lonP = p.Longitude;

            // obliczanie długości segmentu kwadrat (w stopniach)
            double l2 = Math.Pow(lat2 - lat1, 2) + Math.Pow(lon2 - lon1, 2);
            if (l2 == 0.0) return HaversineDistance(latP, lonP, lat1, lon1) * 1000; // v == w, odległość punktu do v

            // parametry t określające projekcję punktu na segment linii
            double t = ((latP - lat1) * (lat2 - lat1) + (lonP - lon1) * (lon2 - lon1)) / l2;
            t = Math.Max(0, Math.Min(1, t));

            // punkt projekcji na segmencie
            double projLat = lat1 + t * (lat2 - lat1);
            double projLon = lon1 + t * (lon2 - lon1);

            // odległość punktu od punktu projekcji (w metrach)
            return HaversineDistance(latP, lonP, projLat, projLon) * 1000;
        }


    }
}