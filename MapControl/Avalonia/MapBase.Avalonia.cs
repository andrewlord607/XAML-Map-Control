using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Styling;

namespace MapControl
{
    public partial class MapBase : IStyleable
    {
        Type IStyleable.StyleKey => typeof(MapBase);

        public static readonly AvaloniaProperty ForegroundProperty =
            TemplatedControl.ForegroundProperty.AddOwner<MapBase>();

        public static readonly AvaloniaProperty CenterProperty = AvaloniaProperty.Register<MapBase, Location>(
            nameof(Center), new Location(), defaultBindingMode: BindingMode.TwoWay);

        public static readonly AvaloniaProperty TargetCenterProperty = AvaloniaProperty.Register(
            nameof(TargetCenter), typeof(Location), typeof(MapBase), new FrameworkPropertyMetadata(
                new Location(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).TargetCenterPropertyChanged((Location)e.NewValue)));

        public static readonly AvaloniaProperty ZoomLevelProperty = AvaloniaProperty.Register(
            nameof(ZoomLevel), typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).ZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly AvaloniaProperty TargetZoomLevelProperty = AvaloniaProperty.Register(
            nameof(TargetZoomLevel), typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).TargetZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly AvaloniaProperty HeadingProperty = AvaloniaProperty.Register(
            nameof(Heading), typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).HeadingPropertyChanged((double)e.NewValue)));

        public static readonly AvaloniaProperty TargetHeadingProperty = AvaloniaProperty.Register(
            nameof(TargetHeading), typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).TargetHeadingPropertyChanged((double)e.NewValue)));

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