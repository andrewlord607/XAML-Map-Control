﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINDOWS_UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Orthographic Projection.
    /// </summary>
    public class OrthographicProjection : AzimuthalProjection
    {
        public OrthographicProjection()
        {
            CrsId = "AUTO2:42003";
        }

        public override Point LocationToMap(Location location)
        {
            if (location.Equals(Center))
            {
                return new Point();
            }

            var lat0 = Center.Latitude * Math.PI / 180d;
            var lat = location.Latitude * Math.PI / 180d;
            var dLon = (location.Longitude - Center.Longitude) * Math.PI / 180d;
            var s = UnitsPerDegree * 180d / Math.PI;

            return new Point(
                s * Math.Cos(lat) * Math.Sin(dLon),
                s * (Math.Cos(lat0) * Math.Sin(lat) - Math.Sin(lat0) * Math.Cos(lat) * Math.Cos(dLon)));
        }

        public override Location MapToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return new Location(Center.Latitude, Center.Longitude);
            }

            var s = UnitsPerDegree * 180d / Math.PI;
            var x = point.X / s;
            var y = point.Y / s;
            var r2 = x * x + y * y;

            if (r2 > 1d)
            {
                return new Location(double.NaN, double.NaN);
            }

            var r = Math.Sqrt(r2);
            var sinC = r;
            var cosC = Math.Sqrt(1 - r2);

            var lat0 = Center.Latitude * Math.PI / 180d;
            var cosLat0 = Math.Cos(lat0);
            var sinLat0 = Math.Sin(lat0);

            return new Location(
                180d / Math.PI * Math.Asin(cosC * sinLat0 + y * sinC * cosLat0 / r),
                180d / Math.PI * Math.Atan2(x * sinC, r * cosC * cosLat0 - y * sinC * sinLat0) + Center.Longitude);
        }
    }
}
