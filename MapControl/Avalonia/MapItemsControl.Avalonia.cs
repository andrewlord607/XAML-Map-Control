using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Styling;

namespace MapControl
{
    public partial class MapItem : IStyleable
    {
        Type IStyleable.StyleKey => typeof(MapBase);

        public static readonly StyledProperty<bool> AutoCollapseProperty = MapPanel.AutoCollapseProperty.AddOwner<MapItem>();

        public static readonly StyledProperty<Location> LocationProperty = MapPanel.LocationProperty.AddOwner<MapItem>();

        public MapItemsControl ParentControl;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                ParentControl?.OnItemClicked(this, e.KeyModifiers.HasFlag(KeyModifiers.Control),
                    e.KeyModifiers.HasFlag(KeyModifiers.Shift));
            }
        }
    }
}