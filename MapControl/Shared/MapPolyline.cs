﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif Avalonia
using Avalonia;
using Avalonia.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// A polyline defined by a collection of Locations.
    /// </summary>
    public class MapPolyline : MapPath
    {
#if !Avalonia
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            nameof(Locations), typeof(IEnumerable<Location>), typeof(MapPolyline),
            new PropertyMetadata(null, (o, e) => ((MapPolyline)o).DataCollectionPropertyChanged(e)));
#else
        public static readonly AvaloniaProperty<IEnumerable<Location>> LocationsProperty = AvaloniaProperty.Register<MapPolyline, IEnumerable<Location>>(
            nameof(Locations));
#endif

#if Avalonia
        static MapPolyline()
        {
            LocationsProperty.Changed.AddClassHandler<MapPolyline>((o, e) =>
                o.DataCollectionPropertyChanged(e));
        }
#endif

        /// <summary>
        /// Gets or sets the Locations that define the polyline points.
        /// </summary>
#if !UWP
        [System.ComponentModel.TypeConverter(typeof(LocationCollectionConverter))]
#endif
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        public MapPolyline()
        {
            Data = new PathGeometry();
        }

        protected override void UpdateData()
        {
            var pathFigures = ((PathGeometry)Data).Figures;
            pathFigures.Clear();

            if (ParentMap != null && Locations != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location ?? Locations.FirstOrDefault());

                AddPolylineLocations(pathFigures, Locations, longitudeOffset, false);
            }
        }
    }
}
