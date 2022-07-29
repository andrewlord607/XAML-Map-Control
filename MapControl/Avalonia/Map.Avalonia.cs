using System;
using Avalonia;
using Avalonia.Input;

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly AvaloniaProperty<double> MouseWheelZoomDeltaProperty = AvaloniaProperty.Register<Map, double>(
            nameof(MouseWheelZoomDelta), 1d);

        private Point? mousePosition;

        static Map()
        {

        }

        public Map()
        {
            PointerPressed += OnMouseLeftButtonDown;
            PointerReleased += OnMouseLeftButtonUp;
            PointerMoved += OnMouseMove;
            PointerWheelChanged += OnMouseWheel;
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes during a MouseWheel event.
        /// The default value is 1.
        /// </summary>
        public double MouseWheelZoomDelta
        {
            get => (double)GetValue(MouseWheelZoomDeltaProperty);
            set => SetValue(MouseWheelZoomDeltaProperty, value);
        }

        private void OnMouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            //if (e.Pointer.Capture())
            if(e.Pointer.Type == PointerType.Mouse
               && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                mousePosition = e.GetPosition(this);
            }
        }

        private void OnMouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Mouse 
                && e.InitialPressMouseButton == MouseButton.Left 
                && mousePosition.HasValue)
            {
                mousePosition = null;
                //ReleaseMouseCapture();
            }
        }

        private void OnMouseMove(object sender, PointerEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Mouse 
                && mousePosition.HasValue)
            {
                var position = e.GetPosition(this);
                var translation = position - mousePosition.Value;
                mousePosition = position;

                TranslateMap(translation);
            }
        }

        private void OnMouseWheel(object sender, PointerWheelEventArgs e)
        {
            if(e.Pointer.Type != PointerType.Mouse)
                return;

            var zoomLevel = TargetZoomLevel + MouseWheelZoomDelta * Math.Sign(e.Delta.Y);

            ZoomMap(e.GetPosition(this), MouseWheelZoomDelta * Math.Round(zoomLevel / MouseWheelZoomDelta));
        }
    }
}