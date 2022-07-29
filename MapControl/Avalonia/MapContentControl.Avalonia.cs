using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using System;

namespace MapControl
{
    /// <summary>
    /// ContentControl placed on a MapPanel at a geographic location specified by the Location property.
    /// </summary>
    public class MapContentControl : ContentControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(MapContentControl);

        public static readonly StyledProperty<bool> AutoCollapseProperty = MapPanel.AutoCollapseProperty.AddOwner<MapContentControl>();

        public static readonly StyledProperty<Location> LocationProperty = MapPanel.LocationProperty.AddOwner<MapContentControl>();

        static MapContentControl()
        {
            
        }

        /// <summary>
        /// Gets/sets MapPanel.AutoCollapse.
        /// </summary>
        public bool AutoCollapse
        {
            get { return (bool)GetValue(AutoCollapseProperty); }
            set { SetValue(AutoCollapseProperty, value); }
        }

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
    }

    /// <summary>
    /// MapContentControl with a Pushpin Style.
    /// </summary>
    public class Pushpin : MapContentControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(Pushpin);

        static Pushpin()
        {
        }

        public static readonly AvaloniaProperty<CornerRadius> CornerRadiusProperty = AvaloniaProperty.Register<Pushpin, CornerRadius>(
            nameof(CornerRadius));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
    }
}