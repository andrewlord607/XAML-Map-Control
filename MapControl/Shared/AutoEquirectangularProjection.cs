﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINDOWS_UWP
using System.Windows;
#endif

namespace MapControl
{
    public class AutoEquirectangularProjection : MapProjection
    {
        public AutoEquirectangularProjection()
        {
            CrsId = "AUTO2:42004";
        }

        public override Point LocationToMap(Location location)
        {
            var xScale = UnitsPerDegree * Math.Cos(Center.Latitude * Math.PI / 180d);

            return new Point(
                xScale * (location.Longitude - Center.Longitude),
                UnitsPerDegree * location.Latitude);
        }

        public override Location MapToLocation(Point point)
        {
            var xScale = UnitsPerDegree * Math.Cos(Center.Latitude * Math.PI / 180d);

            return new Location(
                point.Y / UnitsPerDegree,
                point.X / xScale + Center.Longitude);
        }
    }
}
