using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Styling;
using System;

namespace MapControl
{
    public partial class MapBase : IStyleable
    {
        Type IStyleable.StyleKey => typeof(MapBase);

        public static readonly AvaloniaProperty ForegroundProperty =
            TemplatedControl.ForegroundProperty.AddOwner<MapBase>();

        public static readonly AvaloniaProperty CenterProperty = AvaloniaProperty.Register<MapBase, Location>(
            nameof(Center), new Location(), defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty TargetCenterProperty = AvaloniaProperty.Register<MapBase, Location>(
            nameof(TargetCenter), new Location(), defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty ZoomLevelProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(ZoomLevel), 1d, defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty TargetZoomLevelProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(TargetZoomLevel), 1d, defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty HeadingProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(Heading), 0d, defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty TargetHeadingProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(TargetHeading), 0d, defaultBindingMode: BindingMode.TwoWay);

        private static readonly AvaloniaProperty<double> ViewScaleProperty = AvaloniaProperty.RegisterDirect<MapBase, double>(
            nameof(ViewScale), _ => 0d);

        private static readonly AvaloniaProperty<Point> CenterPointProperty = AvaloniaProperty.Register<MapBase, Point>(
            "CenterPoint");

        private void SetViewScale(double scale)
        {
            SetValue(ViewScaleProperty, scale);
        }
    }
}