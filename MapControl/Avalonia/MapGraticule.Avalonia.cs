using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;

namespace MapControl
{
    public partial class MapGraticule
    {
        static MapGraticule()
        {
            StrokeThicknessProperty.OverrideDefaultValue<MapGraticule>(0.5);
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            InvalidateVisual();
        }

        public override void Render(DrawingContext drawingContext)
        {
            if (ParentMap != null)
            {
                var pathGeometry = new PathGeometry();

                var labels = DrawGraticule(pathGeometry.Figures);

                drawingContext.DrawGeometry(null, CreatePen(), pathGeometry);

                if (labels.Count > 0)
                {
                    // TODO: add FontStretch
                    var typeface = new Typeface(FontFamily, FontStyle, FontWeight);
                    
                    //var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                    foreach (var label in labels)
                    {
                        var latText = new FormattedText
                        {
                            Text = label.LatitudeText,
                            FontSize = FontSize,
                            Typeface = typeface,
                            Spans = new List<FormattedTextStyleSpan>
                            {
                                new(0, label.LatitudeText.Length, Foreground)
                            }
                        };

                        var lonText = new FormattedText
                        {
                            Text = label.LongitudeText,
                            FontSize = FontSize,
                            Typeface = typeface
                        };

                        var x = label.X + StrokeThickness / 2d + 2d;
                        var y1 = label.Y - StrokeThickness / 2d - latText.Bounds.Height;
                        var y2 = label.Y + StrokeThickness / 2d;
                        using (drawingContext.PushSetTransform(Avalonia.Matrix.CreateRotation(label.Rotation))) // TODO: Add translation?
                        {
                            drawingContext.DrawText(Foreground, new Point(x, y1), latText);
                            drawingContext.DrawText(Foreground, new Point(x, y2), lonText);
                        }
                    }
                }
            }
        }

        private static PathFigure CreatePolylineFigure(IEnumerable<Point> points)
        {
            var figure = new PathFigure
            {
                StartPoint = points.First(),
                IsFilled = false
            };

            // TODO: add stroke
            figure.Segments.Add(new PolyLineSegment(points.Skip(1)));
            return figure;
        }
    }
}