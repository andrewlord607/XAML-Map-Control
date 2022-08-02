using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace MapControl
{
    public partial class PushpinBorder : Decorator
    {
        public static readonly AvaloniaProperty<Size> ArrowSizeProperty = AvaloniaProperty.Register<PushpinBorder, Size>(
            nameof(ArrowSize), new Size(10d, 20d));

        public static readonly AvaloniaProperty<double> BorderWidthProperty = AvaloniaProperty.Register<PushpinBorder, double>(
            nameof(BorderWidth));

        public static readonly AvaloniaProperty<Brush> BackgroundProperty = AvaloniaProperty.Register<PushpinBorder, Brush>(
            nameof(Background));

        public static readonly AvaloniaProperty<Brush> BorderBrushProperty = AvaloniaProperty.Register<PushpinBorder, Brush>(
            nameof(BorderBrush));

        public static readonly AvaloniaProperty<CornerRadius> CornerRadiusProperty = AvaloniaProperty.Register<PushpinBorder, CornerRadius>(
            nameof(CornerRadius));

        public static readonly AvaloniaProperty<Thickness> PaddingProperty = AvaloniaProperty.Register<PushpinBorder, Thickness>(
            nameof(Padding), new Thickness(2));

        static PushpinBorder()
        {
            AffectsMeasure<PushpinBorder>(
                ArrowSizeProperty, 
                BorderWidthProperty, 
                BackgroundProperty,
                CornerRadiusProperty,
                PaddingProperty);

            AffectsRender<PushpinBorder>(
                ArrowSizeProperty,
                BorderWidthProperty,
                BackgroundProperty,
                BorderBrushProperty,
                CornerRadiusProperty,
                PaddingProperty);
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var width = 2d * BorderWidth + Padding.Left + Padding.Right;
            var height = 2d * BorderWidth + Padding.Top + Padding.Bottom;

            if (Child != null)
            {
                Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                width += Child.DesiredSize.Width;
                height += Child.DesiredSize.Height;
            }

            var minWidth = BorderWidth + Math.Max(
                CornerRadius.TopLeft + CornerRadius.TopRight,
                CornerRadius.BottomLeft + CornerRadius.BottomRight + ArrowSize.Width);

            var minHeight = BorderWidth + Math.Max(
                CornerRadius.TopLeft + CornerRadius.BottomLeft,
                CornerRadius.TopRight + CornerRadius.BottomRight);

            return new Size(
                Math.Max(width, minWidth),
                Math.Max(height, minHeight) + ArrowSize.Height);
        }

        protected override Size ArrangeOverride(Size size)
        {
            Child?.Arrange(new Rect(
                BorderWidth + Padding.Left,
                BorderWidth + Padding.Top,
                size.Width - BorderWidth - Padding.Right,
                size.Height - BorderWidth - Padding.Bottom));

            return size;
        }

        public override void Render(DrawingContext drawingContext)
        {
            var pen = new Pen
            {
                Brush = BorderBrush,
                Thickness = BorderWidth,
                LineJoin = PenLineJoin.Round
            };

            drawingContext.DrawGeometry(Background, pen, BuildGeometry());

            base.Render(drawingContext);
        }
    }
}