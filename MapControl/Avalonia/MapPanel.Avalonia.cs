using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly StyledProperty<Location> LocationProperty = AvaloniaProperty.RegisterAttached<MapPanel, Location>(
            "Location", typeof(MapPanel));

        public static readonly AvaloniaProperty<BoundingBox> BoundingBoxProperty = AvaloniaProperty.RegisterAttached<MapPanel, BoundingBox>(
            "BoundingBox", typeof(MapPanel));

        private static readonly StyledProperty<MapBase> ParentMapProperty = AvaloniaProperty.RegisterAttached<MapPanel, MapBase>(
            "ParentMap", typeof(MapPanel), null, true, BindingMode.Default);

        private static readonly AvaloniaProperty<Point?> ViewPositionProperty = AvaloniaProperty.RegisterAttached<MapPanel, Point?>(
            "ViewPosition", typeof(MapPanel));

        static MapPanel()
        {
            AffectsArrange<MapPanel>(LocationProperty, BoundingBoxProperty);
            ParentMapProperty.Changed.Subscribe(ParentMapPropertyChanged);
        }

        public MapPanel()
        {
            if (this is MapBase)
            {
                SetValue(ParentMapProperty, this);
            }
        }

        public static MapBase GetParentMap(Control element, LogicalTreeAttachmentEventArgs e)
        {
            var parentMap = element.GetValue(ParentMapProperty);

            if (parentMap == null && (parentMap = FindParentMap(element, e.Parent)) != null)
            {
                element.SetValue(ParentMapProperty, parentMap);
            }

            return parentMap;
        }

        private static MapBase FindParentMap(IAvaloniaObject element, ILogical visualParent)
        {
            return visualParent is Control parent
                ? ((parent as MapBase) ?? element.GetValue(ParentMapProperty) ?? FindParentMap(parent, visualParent.LogicalParent))
                : null;
        }

        private static void SetViewPosition(Control element, Point? viewPosition)
        {
            element.SetValue(ViewPositionProperty, viewPosition);
        }

        public static void InitMapElement(Control element)
        {
            if (element is MapBase)
            {
                element.SetValue(ParentMapProperty, element);
            }
            else
            {
                // Workaround for missing property value inheritance.
                // Loaded and Unloaded handlers set and clear the ParentMap property value.

                element.AttachedToLogicalTree += (s, e) => GetParentMap(element, e); //TODO: LOADED
                element.DetachedFromLogicalTree += (s, e) => element.ClearValue(ParentMapProperty); //TODO: UnLOADED
            }
        }
    }
}