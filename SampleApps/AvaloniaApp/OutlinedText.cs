using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SampleApplication
{
    public class OutlinedText : Control
    {
        private FormattedText text;
        private Geometry outline;

        public static readonly AvaloniaProperty TextProperty = TextBlock.TextProperty.AddOwner<OutlinedText>(outlinedText => outlinedText.Text);

        public static readonly AvaloniaProperty FontSizeProperty = TextBlock.FontSizeProperty.AddOwner<OutlinedText>();

        public static readonly AvaloniaProperty FontFamilyProperty = TextBlock.FontFamilyProperty.AddOwner<OutlinedText>();

        public static readonly AvaloniaProperty FontStyleProperty = TextBlock.FontStyleProperty.AddOwner<OutlinedText>();

        public static readonly AvaloniaProperty FontWeightProperty = TextBlock.FontWeightProperty.AddOwner<OutlinedText>();

        //public static readonly AvaloniaProperty FontStretchProperty = TextBlock.FontStretchProperty.AddOwner<OutlinedText>();

        public static readonly AvaloniaProperty ForegroundProperty = TextBlock.ForegroundProperty.AddOwner<OutlinedText>();

        public static readonly StyledProperty<IBrush> BackgroundProperty = TextBlock.BackgroundProperty.AddOwner<OutlinedText>();

        public static readonly AvaloniaProperty<double> OutlineThicknessProperty = AvaloniaProperty.Register<OutlinedText, double>(
            nameof(OutlineThickness), 1d);

            static OutlinedText()
            {
             AffectsMeasure<OutlinedText>(
                 TextProperty,
                 FontSizeProperty,
                 FontFamilyProperty,
                 FontStyleProperty,
                 FontWeightProperty,
                 //FontStretchProperty,
                 ForegroundProperty,
                 BackgroundProperty,
                 OutlineThicknessProperty);

            BackgroundProperty.OverrideDefaultValue<OutlinedText>(Brushes.White);

             OutlineThicknessProperty.Changed.AddClassHandler<OutlinedText>((o, e) => o.text = null);
             BackgroundProperty.Changed.AddClassHandler<OutlinedText>((o, e) => o.text = null);
             ForegroundProperty.Changed.AddClassHandler<OutlinedText>((o, e) => o.text = null);
             //FontStretchProperty.Changed.AddClassHandler<OutlinedText>((o, e) => o.text = null);
             FontWeightProperty.Changed.AddClassHandler<OutlinedText>((o, e) => o.text = null);
             FontStyleProperty.Changed.AddClassHandler<OutlinedText>((o, e) => o.text = null);
             FontFamilyProperty.Changed.AddClassHandler<OutlinedText>((o, e) => o.text = null);
             FontSizeProperty.Changed.AddClassHandler<OutlinedText>((o, e) => o.text = null);
             TextProperty.Changed.AddClassHandler<OutlinedText>((o, e) => o.text = null);
            }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        //TODO: FontStretch add
        //public FontStretch FontStretch
        //{
        //    get { return (FontStretch)GetValue(FontStretchProperty); }
        //    set { SetValue(FontStretchProperty, value); }
        //}

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public double OutlineThickness
        {
            get { return (double)GetValue(OutlineThicknessProperty); }
            set { SetValue(OutlineThicknessProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return ValidateText() ? outline.Bounds.Size : new Size();
        }

        public override void Render(DrawingContext drawingContext)
        {
            if (ValidateText())
            {
                var location = outline.Bounds.Position;
                drawingContext.PushSetTransform(new TranslateTransform(-location.X, -location.Y).Value);
                drawingContext.DrawGeometry(Background, null, outline);
                drawingContext.DrawText(Foreground, new Point(), text);
            }
        }

        private bool ValidateText()
        {
            if (text == null)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return false;
                }

                var typeface = new Typeface(FontFamily, FontStyle, FontWeight/*, FontStretch*/);

                if (typeface.GlyphTypeface == null)
                {
                    return false;
                }

                text = new FormattedText()
                {
                    Text = Text,
                    FontSize = FontSize,
                    Typeface = typeface,
                };

                //TODO: Build Geometry
                //outline = text.BuildGeometry(new Point()).GetWidenedPathGeometry(
                //    new Pen
                //    {
                //        Thickness = OutlineThickness * 2d,
                //        LineJoin = PenLineJoin.Round,
                //        StartLineCap = PenLineCap.Round,
                //        EndLineCap = PenLineCap.Round
                //    });
            }

            return true;
        }
    }
}