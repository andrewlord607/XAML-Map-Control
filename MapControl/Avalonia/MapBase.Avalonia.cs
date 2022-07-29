using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Styling;
using System;
using Avalonia.Media;

namespace MapControl
{
    public partial class MapBase : IStyleable
    {
        Type IStyleable.StyleKey => typeof(MapBase);

        public static readonly StyledProperty<IBrush> ForegroundProperty =
            TemplatedControl.ForegroundProperty.AddOwner<MapBase>();

        public static readonly AvaloniaProperty<Location> CenterProperty = AvaloniaProperty.Register<MapBase, Location>(
            nameof(Center), new Location(), defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty<Location> TargetCenterProperty = AvaloniaProperty.Register<MapBase, Location>(
            nameof(TargetCenter), new Location(), defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty<double> ZoomLevelProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(ZoomLevel), 1d, defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty TargetZoomLevelProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(TargetZoomLevel), 1d, defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty HeadingProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(Heading), 0d, defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty TargetHeadingProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(TargetHeading), 0d, defaultBindingMode: BindingMode.TwoWay);

        private static readonly AvaloniaProperty<double> ViewScaleProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(ViewScale));

        private static readonly AvaloniaProperty<Point> CenterPointProperty = AvaloniaProperty.Register<MapBase, Point>(
            "CenterPoint");

        private void SetViewScale(double scale)
        {
            SetValue(ViewScaleProperty, scale);
        }
    }
}