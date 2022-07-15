#nullable enable
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using DashStyle = Avalonia.Media.DashStyle;

namespace MapControl
{
    public partial class MapOverlay
    {
        public static readonly StyledProperty<FontFamily> FontFamilyProperty = TextBlock.FontFamilyProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<double> FontSizeProperty = TextBlock.FontSizeProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<FontStyle> FontStyleProperty = TextBlock.FontStyleProperty.AddOwner<MapOverlay>();

        //public static readonly AvaloniaProperty FontStretchProperty = TextBlock.FontStretchProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<FontWeight> FontWeightProperty = TextBlock.FontWeightProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<IBrush> ForegroundProperty = TextBlock.ForegroundProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<IBrush?> StrokeProperty = Shape.StrokeProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<double> StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<AvaloniaList<double>?> StrokeDashArrayProperty = Shape.StrokeDashArrayProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<double> StrokeDashOffsetProperty = Shape.StrokeDashOffsetProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<PenLineCap> StrokeLineCapProperty = Shape.StrokeLineCapProperty.AddOwner<MapOverlay>();

        public static readonly StyledProperty<PenLineJoin> StrokeLineJoinProperty = Shape.StrokeJoinProperty.AddOwner<MapOverlay>();

        //public static readonly StyledProperty<double> StrokeMiterLimitProperty = Shape.StrokeMiterLimitProperty.AddOwner<MapOverlay>();

        static MapOverlay()
        {
            AffectsRender<MapOverlay>(FontFamilyProperty, FontSizeProperty, FontStyleProperty,
                FontWeightProperty, ForegroundProperty, StrokeProperty,
                StrokeThicknessProperty, StrokeDashArrayProperty, StrokeDashOffsetProperty,
                StrokeLineCapProperty, StrokeLineJoinProperty);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // If this.Stroke is not explicitly set, bind it to this.Foreground
            //this.SetBindingOnUnsetProperty(StrokeProperty, this, ForegroundProperty, nameof(Foreground));
            if(!StrokeProperty.IsValidValue(Stroke))
            {
                this.Bind(StrokeProperty, new Binding {Source = ForegroundProperty, Path = nameof(Foreground)});
            }
        }

        public Pen CreatePen()
        {
            return new Pen
            {
                Brush = Stroke,
                Thickness = StrokeThickness,
                LineJoin = StrokeLineJoin,
                //MiterLimit = StrokeMiterLimit,
                LineCap = StrokeLineCap,
                DashStyle = new DashStyle(StrokeDashArray, StrokeDashOffset)
            };
        }
    }
}