using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly StyledProperty<Location> LocationProperty = AvaloniaProperty.RegisterAttached<MapPanel, Location>(
            "Location", typeof(MapPanel));

        public static readonly AvaloniaProperty<BoundingBox> BoundingBoxProperty = AvaloniaProperty.RegisterAttached<MapPanel, BoundingBox>(
            "BoundingBox", typeof(MapPanel));

        private static readonly AvaloniaProperty<MapBase> ParentMapProperty = AvaloniaProperty.RegisterAttached<MapPanel, MapBase>(
            "ParentMap", typeof(MapPanel), null, true, BindingMode.Default);

        private static readonly AvaloniaProperty<Point?> ViewPositionProperty = AvaloniaProperty.RegisterAttached<MapPanel, Point?>(
            "ViewPosition", typeof(MapPanel));

#if Avalonia
        static MapPanel()
        {
            AffectsArrange<MapPanel>(LocationProperty, BoundingBoxProperty);
            ParentMapProperty.Changed.AddClassHandler<MapPanel>(ParentMapPropertyChanged);
        }
#endif

        public MapPanel()
        {
            if (this is MapBase)
            {
                SetValue(ParentMapProperty, this);
            }
        }

        public static MapBase GetParentMap(Control element)
        {
            return (MapBase)element.GetValue(ParentMapProperty);
        }

        private static void SetViewPosition(Control element, Point? viewPosition)
        {
            element.SetValue(ViewPositionProperty, viewPosition);
        }
    }
}