// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif Avalonia
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Optional interface to hold the value of the attached property MapPanel.ParentMap.
    /// </summary>
    public interface IMapElement
    {
        MapBase ParentMap { get; set; }
    }

    /// <summary>
    /// Arranges child elements on a Map at positions specified by the attached property Location,
    /// or in rectangles specified by the attached property BoundingBox.
    /// </summary>
    public partial class MapPanel : Panel, IMapElement
    {
#if !Avalonia
        public static readonly DependencyProperty AutoCollapseProperty = DependencyProperty.RegisterAttached(
            "AutoCollapse", typeof(bool), typeof(MapPanel), new PropertyMetadata(false));
#else
        public static readonly AvaloniaProperty<bool> AutoCollapseProperty =
            AvaloniaProperty.Register<MapPanel, bool>("AutoCollapse");
#endif

        private MapBase parentMap;

        public MapBase ParentMap
        {
            get { return parentMap; }
            set { SetParentMap(value); }
        }

        /// <summary>
        /// Gets a value that controls whether an element's Visibility is automatically
        /// set to Collapsed when it is located outside the visible viewport area.
        /// </summary>
#if !Avalonia
        public static bool GetAutoCollapse(FrameworkElement element)
#else
        public static bool GetAutoCollapse(Control element)
#endif
        {
            return (bool)element.GetValue(AutoCollapseProperty);
        }

        /// <summary>
        /// Sets the AutoCollapse property.
        /// </summary>
#if !Avalonia
        public static void SetAutoCollapse(FrameworkElement element, bool value)
#else
        public static void SetAutoCollapse(Control element, bool value)
#endif
        {
            element.SetValue(AutoCollapseProperty, value);
        }

        /// <summary>
        /// Gets the geodetic Location of an element.
        /// </summary>
#if !Avalonia
        public static Location GetLocation(FrameworkElement element)
#else
        public static Location GetLocation(Control element)
#endif
        {
            return (Location)element.GetValue(LocationProperty);
        }

        /// <summary>
        /// Sets the geodetic Location of an element.
        /// </summary>
#if !Avalonia
        public static void SetLocation(FrameworkElement element, Location value)
#else
        public static void SetLocation(Control element, Location value)
#endif
        {
            element.SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Gets the BoundingBox of an element.
        /// </summary>
#if !Avalonia
        public static BoundingBox GetBoundingBox(FrameworkElement element)
#else
        public static BoundingBox GetBoundingBox(Control element)
#endif
        {
            return (BoundingBox)element.GetValue(BoundingBoxProperty);
        }

        /// <summary>
        /// Sets the BoundingBox of an element.
        /// </summary>
#if !Avalonia
        public static void SetBoundingBox(FrameworkElement element, BoundingBox value)
#else
        public static void SetBoundingBox(Control element, BoundingBox value)
#endif
        {
            element.SetValue(BoundingBoxProperty, value);
        }

        /// <summary>
        /// Gets the position of an element in view coordinates.
        /// </summary>
#if !Avalonia
        public static Point? GetViewPosition(FrameworkElement element)
#else
        public static Point? GetViewPosition(Control element)
#endif
        {
            return (Point?)element.GetValue(ViewPositionProperty);
        }

        /// <summary>
        /// Returns the view position of a Location.
        /// </summary>
        public Point? GetViewPosition(Location location)
        {
            if (location == null)
            {
                return null;
            }

            var position = parentMap.LocationToView(location);

            if (parentMap.MapProjection.Type <= MapProjectionType.NormalCylindrical && IsOutsideViewport(position))
            {
                location = new Location(location.Latitude, parentMap.ConstrainedLongitude(location.Longitude));

                position = parentMap.LocationToView(location);
            }

            return position;
        }

        /// <summary>
        /// Returns the potentially rotated view rectangle of a BoundingBox.
        /// </summary>
        public ViewRect GetViewRect(BoundingBox boundingBox)
        {
            return GetViewRect(parentMap.MapProjection.BoundingBoxToRect(boundingBox));
        }

        /// <summary>
        /// Returns the potentially rotated view rectangle of a map coordinate rectangle.
        /// </summary>
        public ViewRect GetViewRect(Rect rect)
        {
            var center = new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d);
            var position = parentMap.ViewTransform.MapToView(center);

            if (parentMap.MapProjection.Type <= MapProjectionType.NormalCylindrical && IsOutsideViewport(position))
            {
                var location = parentMap.MapProjection.MapToLocation(center);
                if (location != null)
                {
                    location.Longitude = parentMap.ConstrainedLongitude(location.Longitude);
                    position = parentMap.LocationToView(location);
                }
            }

            var width = rect.Width * parentMap.ViewTransform.Scale;
            var height = rect.Height * parentMap.ViewTransform.Scale;
            var x = position.X - width / 2d;
            var y = position.Y - height / 2d;

            return new ViewRect(x, y, width, height, parentMap.ViewTransform.Rotation);
        }

        protected virtual void SetParentMap(MapBase map)
        {
            if (parentMap != null && parentMap != this)
            {
                parentMap.ViewportChanged -= OnViewportChanged;
            }

            parentMap = map;

            if (parentMap != null && parentMap != this)
            {
                parentMap.ViewportChanged += OnViewportChanged;

                OnViewportChanged(new ViewportChangedEventArgs());
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            OnViewportChanged(e);
        }

        protected virtual void OnViewportChanged(ViewportChangedEventArgs e)
        {
            InvalidateArrange();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

#if !Avalonia
            foreach (var element in Children.OfType<FrameworkElement>())
#else
            foreach (var element in Children.OfType<Control>())
#endif
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (parentMap != null)
            {
#if !Avalonia
                foreach (var element in Children.OfType<FrameworkElement>())
#else
                foreach (var element in Children.OfType<Control>())
#endif
                {
                    var position = GetViewPosition(GetLocation(element));

                    SetViewPosition(element, position);

                    if (GetAutoCollapse(element))
                    {
                        if (position.HasValue && IsOutsideViewport(position.Value))
                        {
#if !Avalonia
                            element.SetValue(VisibilityProperty, Visibility.Collapsed);
#else
                            element.SetValue(IsVisibleProperty, false);
#endif
                        }
                        else
                        {
#if !Avalonia
                            element.ClearValue(VisibilityProperty);
#else
                            element.ClearValue(IsVisibleProperty);
#endif
                        }
                    }

                    try
                    {
                        if (position.HasValue)
                        {
#if !Avalonia
                            ArrangeElement(element, position.Value);
#else
                            ArrangeElement(element, position.Value);
#endif
                        }
                        else
                        {
                            var boundingBox = GetBoundingBox(element);

                            if (boundingBox != null)
                            {
                                ArrangeElement(element, GetViewRect(boundingBox));
                            }
                            else
                            {
                                ArrangeElement(element, finalSize);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"MapPanel.ArrangeElement: {ex.Message}");
                    }
                }
            }

            return finalSize;
        }

        private bool IsOutsideViewport(Point point)
        {
            #if !Avalonia
            return point.X < 0d || point.X > parentMap.RenderSize.Width
                || point.Y < 0d || point.Y > parentMap.RenderSize.Height;
            #else
            return point.X < 0d || point.X > parentMap.Bounds.Size.Width
                || point.Y < 0d || point.Y > parentMap.Bounds.Size.Height;
            #endif
        }

#if !Avalonia
        private static void ArrangeElement(FrameworkElement element, ViewRect rect)
#else
        private static void ArrangeElement(Control element, ViewRect rect)
#endif
        {
            element.Width = rect.Width;
            element.Height = rect.Height;

            ArrangeElement(element, new Rect(rect.X, rect.Y, rect.Width, rect.Height));

            if (element.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.Angle = rect.Rotation;
            }
            else if (rect.Rotation != 0d)
            {
                rotateTransform = new RotateTransform { Angle = rect.Rotation };
                element.RenderTransform = rotateTransform;
#if !Avalonia
                element.RenderTransformOrigin = new Point(0.5, 0.5);
#else
                element.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
#endif
            }
        }

#if !Avalonia
        private static void ArrangeElement(FrameworkElement element, Point position)
#else
        private static void ArrangeElement(Control element, Point position)
#endif
        {
            var rect = new Rect(position, element.DesiredSize);

            switch (element.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
#if !Avalonia
                    rect.X -= rect.Width / 2d;
#else
                    rect = rect.WithX(-rect.Width / 2d);
#endif
                    break;

                case HorizontalAlignment.Right:
#if !Avalonia
                    rect.X -= rect.Width;
#else
                    rect = rect.WithX(-rect.Width);
#endif
                    break;

                default:
                    break;
            }

            switch (element.VerticalAlignment)
            {
                case VerticalAlignment.Center:
#if !Avalonia
                    rect.Y -= rect.Height / 2d;
#else
                    rect = rect.WithY(-rect.Height / 2d);
#endif
                    break;

                case VerticalAlignment.Bottom:
#if !Avalonia
                    rect.Y -= rect.Height;
#else
                    rect = rect.WithY(-rect.Height);
#endif
                    break;

                default:
                    break;
            }

            ArrangeElement(element, rect);
        }

#if !Avalonia
        private static void ArrangeElement(FrameworkElement element, Size parentSize)
#else
        private static void ArrangeElement(Control element, Size parentSize)
#endif
        {
            var rect = new Rect(new Point(), element.DesiredSize);

            switch (element.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
#if !Avalonia
                    rect.X = (parentSize.Width - rect.Width) / 2d;
#else
                    rect = rect.WithX((parentSize.Width - rect.Width) / 2d);
#endif
                    break;

                case HorizontalAlignment.Right:
#if !Avalonia
                    rect.X = parentSize.Width - rect.Width;
#else
                    rect = rect.WithX(parentSize.Width - rect.Width);
#endif
                    break;

                case HorizontalAlignment.Stretch:
#if !Avalonia
                    rect.Width = parentSize.Width;
#else
                    rect = rect.WithWidth(parentSize.Width);
#endif
                    break;

                default:
                    break;
            }

            switch (element.VerticalAlignment)
            {
                case VerticalAlignment.Center:
#if !Avalonia
                    rect.Y = (parentSize.Height - rect.Height) / 2d;
#else
                    rect = rect.WithY((parentSize.Height - rect.Height) / 2d);
#endif
                    break;

                case VerticalAlignment.Bottom:
#if !Avalonia
                    rect.Y = parentSize.Height - rect.Height;
#else
                    rect = rect.WithY(parentSize.Height - rect.Height);
#endif
                    break;

                case VerticalAlignment.Stretch:
#if !Avalonia
                    rect.Height = parentSize.Height;
#else
                    rect = rect.WithHeight(parentSize.Height);
#endif
                    break;

                default:
                    break;
            }

            ArrangeElement(element, rect);
        }

#if !Avalonia
        private static void ArrangeElement(FrameworkElement element, Rect rect)
#else
        private static void ArrangeElement(Control element, Rect rect)
#endif
        {
            if (element.UseLayoutRounding)
            {
#if !Avalonia
                rect.X = Math.Round(rect.X);
                rect.Y = Math.Round(rect.Y);
                rect.Width = Math.Round(rect.Width);
                rect.Height = Math.Round(rect.Height);
#else
                rect = new Rect(
                    Math.Round(rect.X),
                    Math.Round(rect.Y),
                    Math.Round(rect.Width),
                    Math.Round(rect.Height));
#endif
            }

            element.Arrange(rect);
        }

#if !Avalonia
        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
#else
        private static void ParentMapPropertyChanged(MapPanel obj, AvaloniaPropertyChangedEventArgs e)
#endif
        {
            if (obj is IMapElement mapElement)
            {
                mapElement.ParentMap = e.NewValue as MapBase;
            }
        }
    }
}
