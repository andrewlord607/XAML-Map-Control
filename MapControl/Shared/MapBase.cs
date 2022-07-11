// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#elif Avalonia
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Styling;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
#endif

namespace MapControl
{
    public interface IMapLayer : IMapElement
    {
        Brush MapBackground { get; }
        Brush MapForeground { get; }
    }

    /// <summary>
    /// The map control. Displays map content provided by one or more tile or image layers,
    /// i.e. MapTileLayerBase or MapImageLayer instances.
    /// The visible map area is defined by the Center and ZoomLevel properties.
    /// The map can be rotated by an angle that is given by the Heading property.
    /// MapBase can contain map overlay child elements like other MapPanels or MapItemsControls.
    /// </summary>
    public partial class MapBase : MapPanel
    {
        public static TimeSpan ImageFadeDuration { get; set; } = TimeSpan.FromSeconds(0.1);

#if !Avalonia
        public static readonly DependencyProperty MapLayerProperty = DependencyProperty.Register(
            nameof(MapLayer), typeof(UIElement), typeof(MapBase),
            new PropertyMetadata(null, (o, e) => ((MapBase)o).MapLayerPropertyChanged((UIElement)e.OldValue, (UIElement)e.NewValue)));
#else
        public static readonly AvaloniaProperty<Control> MapLayerProperty = AvaloniaProperty.Register<MapBase, Control>(
            nameof(MapLayer));
#endif

#if !Avalonia
        public static readonly DependencyProperty MapProjectionProperty = DependencyProperty.Register(
            nameof(MapProjection), typeof(MapProjection), typeof(MapBase),
            new PropertyMetadata(new WebMercatorProjection(), (o, e) => ((MapBase)o).MapProjectionPropertyChanged((MapProjection)e.NewValue)));
#else
        public static readonly AvaloniaProperty<MapProjection> MapProjectionProperty = AvaloniaProperty.Register<MapBase, MapProjection>(
            nameof(MapProjection), new WebMercatorProjection());
#endif

#if !Avalonia
        public static readonly DependencyProperty ProjectionCenterProperty = DependencyProperty.Register(
            nameof(ProjectionCenter), typeof(Location), typeof(MapBase),
            new PropertyMetadata(null, (o, e) => ((MapBase)o).ProjectionCenterPropertyChanged()));
#else
        public static readonly AvaloniaProperty<Location> ProjectionCenterProperty = AvaloniaProperty.Register<MapBase, Location>(
            nameof(ProjectionCenter));
#endif

#if !Avalonia
        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            nameof(MinZoomLevel), typeof(double), typeof(MapBase),
            new PropertyMetadata(1d, (o, e) => ((MapBase)o).MinZoomLevelPropertyChanged((double)e.NewValue)));
#else
        public static readonly AvaloniaProperty<double> MinZoomLevelProperty = AvaloniaProperty.Register<MapBase, double>(
            nameof(MinZoomLevel), 1d);
#endif

#if !Avalonia
        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            nameof(MaxZoomLevel), typeof(double), typeof(MapBase),
            new PropertyMetadata(20d, (o, e) => ((MapBase)o).MaxZoomLevelPropertyChanged((double)e.NewValue)));
#else
        public static readonly AvaloniaProperty<double> MaxZoomLevelProperty =
            AvaloniaProperty.Register<MapBase, double>(
                nameof(MaxZoomLevel), 20d);
#endif


#if !Avalonia
        public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register(
            nameof(AnimationDuration), typeof(TimeSpan), typeof(MapBase),
            new PropertyMetadata(TimeSpan.FromSeconds(0.3)));
#else
        public static readonly AvaloniaProperty<TimeSpan> AnimationDurationProperty = AvaloniaProperty.Register<MapBase, TimeSpan>(
            nameof(AnimationDuration), TimeSpan.FromSeconds(0.3));
#endif

#if !Avalonia
        public static readonly DependencyProperty AnimationEasingFunctionProperty = DependencyProperty.Register(
            nameof(AnimationEasingFunction), typeof(EasingFunctionBase), typeof(MapBase),
            new PropertyMetadata(new QuadraticEase { EasingMode = EasingMode.EaseOut }));
#else
        public static readonly AvaloniaProperty<Easing> AnimationEasingFunctionProperty = AvaloniaProperty.Register<MapBase, Easing>(
            nameof(AnimationEasingFunction), new QuadraticEaseOut());
#endif

#if Avalonia
        static MapBase()
        {
            MapLayerProperty.Changed.AddClassHandler<MapBase>((x, args) =>
                x.MapLayerPropertyChanged((Control)args.OldValue, (Control)args.NewValue));
            MapProjectionProperty.Changed.AddClassHandler<MapBase>((o, e) =>
                o.MapProjectionPropertyChanged((MapProjection)e.NewValue));
            ProjectionCenterProperty.Changed.AddClassHandler<MapBase>((o, e) => o.ProjectionCenterPropertyChanged());
            MinZoomLevelProperty.Changed.AddClassHandler<MapBase>((o, e) =>
                o.MinZoomLevelPropertyChanged((double)e.NewValue));
            MaxZoomLevelProperty.Changed.AddClassHandler<MapBase>((o, e) =>
                o.MaxZoomLevelPropertyChanged((double)e.NewValue));

            CenterProperty.Changed.AddClassHandler<MapBase>((o, e) =>
                ((MapBase)o).CenterPropertyChanged((Location)e.NewValue));

            ClipToBoundsProperty.OverrideMetadata(typeof(MapBase), new StyledPropertyMetadata<bool>(true));

            BoundsProperty.Changed.AddClassHandler<MapBase>((o, _) =>
            {
                o.ResetTransformCenter();
                o.UpdateTransform();
            });

            CenterProperty.Changed.AddClassHandler<MapBase>((o, e) =>
            {
                var center = (Point)e.NewValue;
                o.CenterPointPropertyChanged(new Location(center.Y, center.X));
            });
        }
#endif

#if!Avalonia
        private PointAnimation centerAnimation;
        private DoubleAnimation zoomLevelAnimation;
        private DoubleAnimation headingAnimation;
#else
        private Animation centerAnimation;
        private Animation zoomLevelAnimation;
        private Animation headingAnimation;
#endif
        private Location transformCenter;
        private Point viewCenter;
        private double centerLongitude;
        private double maxLatitude = 90d;
        private bool internalPropertyChange;

        /// <summary>
        /// Raised when the current map viewport has changed.
        /// </summary>
        public event EventHandler<ViewportChangedEventArgs> ViewportChanged;

        /// <summary>
        /// Gets or sets the map foreground Brush.
        /// </summary>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the base map layer, which is added as first element to the Children collection.
        /// If the layer implements IMapLayer (like MapTileLayer or MapImageLayer), its (non-null) MapBackground
        /// and MapForeground property values are used for the MapBase Background and Foreground properties.
        /// </summary>
#if !Avalonia
        public UIElement MapLayer
        {
            get { return (UIElement)GetValue(MapLayerProperty); }
            set { SetValue(MapLayerProperty, value); }
        }
#else
        public Control MapLayer
        {
            get { return (Control)GetValue(MapLayerProperty); }
            set { SetValue(MapLayerProperty, value); }
        }
#endif

        /// <summary>
        /// Gets or sets the MapProjection used by the map control.
        /// </summary>
        public MapProjection MapProjection
        {
            get { return (MapProjection)GetValue(MapProjectionProperty); }
            set { SetValue(MapProjectionProperty, value); }
        }

        /// <summary>
        /// Gets or sets an optional center (reference point) for azimuthal projections.
        /// If ProjectionCenter is null, the Center property value will be used instead.
        /// </summary>
        public Location ProjectionCenter
        {
            get { return (Location)GetValue(ProjectionCenterProperty); }
            set { SetValue(ProjectionCenterProperty, value); }
        }

        /// <summary>
        /// Gets or sets the location of the center point of the map.
        /// </summary>
        public Location Center
        {
            get { return (Location)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        /// <summary>
        /// Gets or sets the target value of a Center animation.
        /// </summary>
        public Location TargetCenter
        {
            get { return (Location)GetValue(TargetCenterProperty); }
            set { SetValue(TargetCenterProperty, value); }
        }

        /// <summary>
        /// Gets or sets the minimum value of the ZoomLevel and TargetZommLevel properties.
        /// Must be greater than or equal to zero and less than or equal to MaxZoomLevel.
        /// The default value is 1.
        /// </summary>
        public double MinZoomLevel
        {
            get { return (double)GetValue(MinZoomLevelProperty); }
            set { SetValue(MinZoomLevelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the maximum value of the ZoomLevel and TargetZommLevel properties.
        /// Must be greater than or equal to MinZoomLevel and less than or equal to 22.
        /// The default value is 20.
        /// </summary>
        public double MaxZoomLevel
        {
            get { return (double)GetValue(MaxZoomLevelProperty); }
            set { SetValue(MaxZoomLevelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the map zoom level.
        /// </summary>
        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the target value of a ZoomLevel animation.
        /// </summary>
        public double TargetZoomLevel
        {
            get { return (double)GetValue(TargetZoomLevelProperty); }
            set { SetValue(TargetZoomLevelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the map heading, i.e. a clockwise rotation angle in degrees.
        /// </summary>
        public double Heading
        {
            get { return (double)GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the target value of a Heading animation.
        /// </summary>
        public double TargetHeading
        {
            get { return (double)GetValue(TargetHeadingProperty); }
            set { SetValue(TargetHeadingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Duration of the Center, ZoomLevel and Heading animations.
        /// The default value is 0.3 seconds.
        /// </summary>
        public TimeSpan AnimationDuration
        {
            get { return (TimeSpan)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        /// <summary>
        /// Gets or sets the EasingFunction of the Center, ZoomLevel and Heading animations.
        /// The default value is a QuadraticEase with EasingMode.EaseOut.
        /// </summary>

#if !Avalonia
        public EasingFunctionBase AnimationEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(AnimationEasingFunctionProperty); }
            set { SetValue(AnimationEasingFunctionProperty, value); }
        }
#else
        public Easing AnimationEasingFunction
        {
            get { return (Easing)GetValue(AnimationEasingFunctionProperty); }
            set { SetValue(AnimationEasingFunctionProperty, value); }
        }
#endif

        /// <summary>
        /// Gets the scaling factor from cartesian map coordinates to view coordinates,
        /// i.e. pixels per meter, as a read-only dependency property.
        /// </summary>
        public double ViewScale
        {
            get { return (double)GetValue(ViewScaleProperty); }
        }

        /// <summary>
        /// Gets the ViewTransform instance that is used to transform between cartesian map coordinates
        /// and view coordinates.
        /// </summary>
        public ViewTransform ViewTransform { get; } = new ViewTransform();

        /// <summary>
        /// Gets the horizontal and vertical scaling factors from cartesian map coordinates to view
        /// coordinates at the specified location, i.e. pixels per meter.
        /// </summary>
        public Vector GetScale(Location location)
        {
            return ViewTransform.Scale * MapProjection.GetRelativeScale(location);
        }

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in view coordinates.
        /// </summary>
        public Point LocationToView(Location location)
        {
            return ViewTransform.MapToView(MapProjection.LocationToMap(location));
        }

        /// <summary>
        /// Transforms a Point in view coordinates to a Location in geographic coordinates.
        /// </summary>
        public Location ViewToLocation(Point point)
        {
            return MapProjection.MapToLocation(ViewTransform.ViewToMap(point));
        }

        /// <summary>
        /// Transforms a Rect in view coordinates to a BoundingBox in geographic coordinates.
        /// </summary>
        public BoundingBox ViewRectToBoundingBox(Rect rect)
        {
            var p1 = ViewTransform.ViewToMap(new Point(rect.X, rect.Y));
            var p2 = ViewTransform.ViewToMap(new Point(rect.X, rect.Y + rect.Height));
            var p3 = ViewTransform.ViewToMap(new Point(rect.X + rect.Width, rect.Y));
            var p4 = ViewTransform.ViewToMap(new Point(rect.X + rect.Width, rect.Y + rect.Height));

#if !Avalonia
            rect.X = Math.Min(p1.X, Math.Min(p2.X, Math.Min(p3.X, p4.X)));
            rect.Y = Math.Min(p1.Y, Math.Min(p2.Y, Math.Min(p3.Y, p4.Y)));
            rect.Width = Math.Max(p1.X, Math.Max(p2.X, Math.Max(p3.X, p4.X))) - rect.X;
            rect.Height = Math.Max(p1.Y, Math.Max(p2.Y, Math.Max(p3.Y, p4.Y))) - rect.Y;
#else
            rect = new Rect(Math.Min(p1.X, Math.Min(p2.X, Math.Min(p3.X, p4.X))),
                Math.Min(p1.Y, Math.Min(p2.Y, Math.Min(p3.Y, p4.Y))),
                Math.Max(p1.X, Math.Max(p2.X, Math.Max(p3.X, p4.X))) - rect.X,
                Math.Max(p1.Y, Math.Max(p2.Y, Math.Max(p3.Y, p4.Y))) - rect.Y);
#endif

            return MapProjection.RectToBoundingBox(rect);
        }

        /// <summary>
        /// Sets a temporary center point in view coordinates for scaling and rotation transformations.
        /// This center point is automatically reset when the Center property is set by application code.
        /// </summary>
        public void SetTransformCenter(Point center)
        {
            transformCenter = ViewToLocation(center);
#if !Avalonia
            viewCenter = transformCenter != null ? center : new Point(RenderSize.Width / 2d, RenderSize.Height / 2d);
#else
            viewCenter = transformCenter != null ? center : new Point(Bounds.Width / 2d, Bounds.Height / 2d);
#endif
        }

        /// <summary>
        /// Resets the temporary transform center point set by SetTransformCenter.
        /// </summary>
        public void ResetTransformCenter()
        {
            transformCenter = null;
#if !Avalonia
            viewCenter = new Point(RenderSize.Width / 2d, RenderSize.Height / 2d);
#else
            viewCenter = new Point(Bounds.Width / 2d, Bounds.Height / 2d);
#endif
        }

        /// <summary>
        /// Changes the Center property according to the specified translation in view coordinates.
        /// </summary>
        public void TranslateMap(Vector translation)
        {
            if (transformCenter != null)
            {
                ResetTransformCenter();
                UpdateTransform();
            }

            if (translation.X != 0d || translation.Y != 0d)
            {
                var center = ViewToLocation(viewCenter - translation);
                if (center != null)
                {
                    Center = center;
                }
            }
        }

        /// <summary>
        /// Changes the Center, Heading and ZoomLevel properties according to the specified
        /// view coordinate translation, rotation and scale delta values. Rotation and scaling
        /// is performed relative to the specified center point in view coordinates.
        /// </summary>
        public void TransformMap(Point center, Vector translation, double rotation, double scale)
        {
            if (rotation != 0d || scale != 1d)
            {
                SetTransformCenter(center);
                viewCenter += translation;

                if (rotation != 0d)
                {
                    var heading = (((Heading + rotation) % 360d) + 360d) % 360d;

                    SetValueInternal(HeadingProperty, heading);
                    SetValueInternal(TargetHeadingProperty, heading);
                }

                if (scale != 1d)
                {
                    var zoomLevel = Math.Min(Math.Max(ZoomLevel + Math.Log(scale, 2d), MinZoomLevel), MaxZoomLevel);

                    SetValueInternal(ZoomLevelProperty, zoomLevel);
                    SetValueInternal(TargetZoomLevelProperty, zoomLevel);
                }

                UpdateTransform(true);
            }
            else
            {
                TranslateMap(translation); // more precise
            }
        }

        /// <summary>
        /// Sets the value of the TargetZoomLevel property while retaining the specified center point
        /// in view coordinates.
        /// </summary>
        public void ZoomMap(Point center, double zoomLevel)
        {
            zoomLevel = Math.Min(Math.Max(zoomLevel, MinZoomLevel), MaxZoomLevel);

            if (TargetZoomLevel != zoomLevel)
            {
                SetTransformCenter(center);
                TargetZoomLevel = zoomLevel;
            }
        }

        /// <summary>
        /// Sets the TargetZoomLevel and TargetCenter properties so that the specified bounding box
        /// fits into the current view. The TargetHeading property is set to zero.
        /// </summary>
        public void ZoomToBounds(BoundingBox boundingBox)
        {
            var rect = MapProjection.BoundingBoxToRect(boundingBox);
            var center = new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d);
            var targetCenter = MapProjection.MapToLocation(center);

            if (targetCenter != null)
            {
#if  !Avalonia
                var scale = Math.Min(RenderSize.Width / rect.Width, RenderSize.Height / rect.Height);

#else
                var scale = Math.Min(Bounds.Width / rect.Width, Bounds.Height / rect.Height);
#endif
                TargetZoomLevel = ViewTransform.ScaleToZoomLevel(scale);
                TargetCenter = targetCenter;
                TargetHeading = 0d;
            }
        }

        internal double ConstrainedLongitude(double longitude)
        {
            var offset = longitude - Center.Longitude;

            if (offset > 180d)
            {
                longitude = Center.Longitude - 360d + offset % 360d;
            }
            else if (offset < -180d)
            {
                longitude = Center.Longitude + 360d + offset % 360d;
            }

            return longitude;
        }

#if !Avalonia
        private void MapLayerPropertyChanged(UIElement oldLayer, UIElement newLayer)
#else
        private void MapLayerPropertyChanged(Control oldLayer, Control newLayer)
#endif
        {
            if (oldLayer != null)
            {
                Children.Remove(oldLayer);

                if (oldLayer is IMapLayer mapLayer)
                {
                    if (mapLayer.MapBackground != null)
                    {
                        ClearValue(BackgroundProperty);
                    }
                    if (mapLayer.MapForeground != null)
                    {
                        ClearValue(ForegroundProperty);
                    }
                }
            }

            if (newLayer != null)
            {
                Children.Insert(0, newLayer);

                if (newLayer is IMapLayer mapLayer)
                {
                    if (mapLayer.MapBackground != null)
                    {
                        Background = mapLayer.MapBackground;
                    }
                    if (mapLayer.MapForeground != null)
                    {
                        Foreground = mapLayer.MapForeground;
                    }
                }
            }
        }

        private void MapProjectionPropertyChanged(MapProjection projection)
        {
            maxLatitude = 90d;

            if (projection.Type <= MapProjectionType.NormalCylindrical)
            {
                var maxLocation = projection.MapToLocation(new Point(0d, 180d * MapProjection.Wgs84MeterPerDegree));

                if (maxLocation != null && maxLocation.Latitude < 90d)
                {
                    maxLatitude = maxLocation.Latitude;

                    var center = Center;
                    AdjustCenterProperty(CenterProperty, ref center);
                }
            }

            ResetTransformCenter();
            UpdateTransform(false, true);
        }

        private void ProjectionCenterPropertyChanged()
        {
            ResetTransformCenter();
            UpdateTransform();
        }

#if !Avalonia
        private void AdjustCenterProperty(DependencyProperty property, ref Location center)
#else
        private void AdjustCenterProperty(AvaloniaProperty property, ref Location center)
#endif
        {
            var c = center;

            if (center == null)
            {
                center = new Location();
            }
            else if (
                center.Latitude < -maxLatitude || center.Latitude > maxLatitude ||
                center.Longitude < -180d || center.Longitude > 180d)
            {
                center = new Location(
                    Math.Min(Math.Max(center.Latitude, -maxLatitude), maxLatitude),
                    Location.NormalizeLongitude(center.Longitude));
            }

            if (center != c)
            {
                SetValueInternal(property, center);
            }
        }

        private void CenterPropertyChanged(Location center)
        {
            if (!internalPropertyChange)
            {
                AdjustCenterProperty(CenterProperty, ref center);
                UpdateTransform();

                if (centerAnimation == null)
                {
                    SetValueInternal(TargetCenterProperty, center);
                }
            }
        }

        private void TargetCenterPropertyChanged(Location targetCenter)
        {
            if (!internalPropertyChange)
            {
                AdjustCenterProperty(TargetCenterProperty, ref targetCenter);

                if (!targetCenter.Equals(Center))
                {
#if !Avalonia
                    if (centerAnimation != null)
                    {
                        centerAnimation.Completed -= CenterAnimationCompleted;
                    }


                    centerAnimation = new PointAnimation
                    {
                        From = new Point(Center.Longitude, Center.Latitude),
                        To = new Point(ConstrainedLongitude(targetCenter.Longitude), targetCenter.Latitude),
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEasingFunction
                    };

                    centerAnimation.Completed += CenterAnimationCompleted;

                    this.BeginAnimation(CenterPointProperty, centerAnimation);
#else
                    centerAnimation = new Animation
                    {
                        Duration = AnimationDuration,
                        Easing = AnimationEasingFunction,
                        Children =
                        {
                            new KeyFrame
                            {
                                KeyTime = TimeSpan.Zero,
                                Setters = { new Setter {Property = CenterProperty, Value = new Point(Center.Longitude, Center.Latitude)} }
                            },
                            new KeyFrame
                            {
                                KeyTime = AnimationDuration,
                                Setters = { new Setter {Property = CenterProperty, Value = new Point(ConstrainedLongitude(targetCenter.Longitude), targetCenter.Latitude)} }
                            }
                        }
                    };
                    centerAnimation.RunAsync(this, null);
#endif
                }
            }
        }

#if !Avalonia
        private void CenterAnimationCompleted(object sender, object e)
        {
            if (centerAnimation != null)
            {
                centerAnimation.Completed -= CenterAnimationCompleted;
                centerAnimation = null;

                this.BeginAnimation(CenterPointProperty, null);
            }
        }
#endif

        private void CenterPointPropertyChanged(Location center)
        {
            if (centerAnimation != null)
            {
                SetValueInternal(CenterProperty, center);
                UpdateTransform();
            }
        }


        private void MinZoomLevelPropertyChanged(double minZoomLevel)
        {
            if (minZoomLevel < 0d || minZoomLevel > MaxZoomLevel)
            {
                minZoomLevel = Math.Min(Math.Max(minZoomLevel, 0d), MaxZoomLevel);

                SetValueInternal(MinZoomLevelProperty, minZoomLevel);
            }

            if (ZoomLevel < minZoomLevel)
            {
                ZoomLevel = minZoomLevel;
            }
        }

        private void MaxZoomLevelPropertyChanged(double maxZoomLevel)
        {
            if (maxZoomLevel < MinZoomLevel)
            {
                maxZoomLevel = MinZoomLevel;

                SetValueInternal(MaxZoomLevelProperty, maxZoomLevel);
            }

            if (ZoomLevel > maxZoomLevel)
            {
                ZoomLevel = maxZoomLevel;
            }
        }

#if !Avalonia
        private void AdjustZoomLevelProperty(DependencyProperty property, ref double zoomLevel)
#else
        private void AdjustZoomLevelProperty(AvaloniaProperty property, ref double zoomLevel)
#endif
        {
            if (zoomLevel < MinZoomLevel || zoomLevel > MaxZoomLevel)
            {
                zoomLevel = Math.Min(Math.Max(zoomLevel, MinZoomLevel), MaxZoomLevel);

                SetValueInternal(property, zoomLevel);
            }
        }

        private void ZoomLevelPropertyChanged(double zoomLevel)
        {
            if (!internalPropertyChange)
            {
                AdjustZoomLevelProperty(ZoomLevelProperty, ref zoomLevel);
                UpdateTransform();

                if (zoomLevelAnimation == null)
                {
                    SetValueInternal(TargetZoomLevelProperty, zoomLevel);
                }
            }
        }

        private void TargetZoomLevelPropertyChanged(double targetZoomLevel)
        {
            if (!internalPropertyChange)
            {
                AdjustZoomLevelProperty(TargetZoomLevelProperty, ref targetZoomLevel);

                if (targetZoomLevel != ZoomLevel)
                {
#if !Avalonia
                    if (zoomLevelAnimation != null)
                    {
                        zoomLevelAnimation.Completed -= ZoomLevelAnimationCompleted;
                    }

                    zoomLevelAnimation = new DoubleAnimation
                    {
                        To = targetZoomLevel,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEasingFunction
                    };

                    zoomLevelAnimation.Completed += ZoomLevelAnimationCompleted;

                    this.BeginAnimation(ZoomLevelProperty, zoomLevelAnimation);
#else
                    zoomLevelAnimation = new Animation
                    {
                        Easing = AnimationEasingFunction,
                        Duration = AnimationDuration,
                        Children =
                        {
                            new KeyFrame
                            {
                                KeyTime = AnimationDuration,
                                Setters = { new Setter {Property = ZoomLevelProperty, Value = targetZoomLevel} }
                            }
                        }
                    };
                    zoomLevelAnimation.Apply(this, null, Observable.Return(true),
                        () => ZoomLevelAnimationCompleted(null, null));
#endif
                }
            }
        }

        private void ZoomLevelAnimationCompleted(object sender, object e)
        {
            if (zoomLevelAnimation != null)
            {
                SetValueInternal(ZoomLevelProperty, TargetZoomLevel);
                UpdateTransform(true);

#if !Avalonia
                zoomLevelAnimation.Completed -= ZoomLevelAnimationCompleted;
                zoomLevelAnimation = null;

                this.BeginAnimation(ZoomLevelProperty, null);
#endif
            }
        }

#if !Avalonia
        private void AdjustHeadingProperty(DependencyProperty property, ref double heading)
#else
        private void AdjustHeadingProperty(AvaloniaProperty property, ref double heading)
#endif
        {
            if (heading < 0d || heading > 360d)
            {
                heading = ((heading % 360d) + 360d) % 360d;

                SetValueInternal(property, heading);
            }
        }

        private void HeadingPropertyChanged(double heading)
        {
            if (!internalPropertyChange)
            {
                AdjustHeadingProperty(HeadingProperty, ref heading);
                UpdateTransform();

                if (headingAnimation == null)
                {
                    SetValueInternal(TargetHeadingProperty, heading);
                }
            }
        }

        private void TargetHeadingPropertyChanged(double targetHeading)
        {
            if (!internalPropertyChange)
            {
                AdjustHeadingProperty(TargetHeadingProperty, ref targetHeading);

                if (targetHeading != Heading)
                {
                    var delta = targetHeading - Heading;

                    if (delta > 180d)
                    {
                        delta -= 360d;
                    }
                    else if (delta < -180d)
                    {
                        delta += 360d;
                    }

#if !Avalonia
                    if (headingAnimation != null)
                    {
                        headingAnimation.Completed -= HeadingAnimationCompleted;
                    }

                    headingAnimation = new DoubleAnimation
                    {
                        By = delta,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEasingFunction
                    };

                    headingAnimation.Completed += HeadingAnimationCompleted;

                    this.BeginAnimation(HeadingProperty, headingAnimation);
#else
                    headingAnimation = new Animation
                    {
                        Easing = AnimationEasingFunction,
                        Duration = AnimationDuration,
                        Children =
                        {
                            new KeyFrame
                            {
                                KeyTime = AnimationDuration,
                                Setters = { new Setter { Property = HeadingProperty, Value = Heading + delta } }
                            }
                        }
                    };
                    headingAnimation.Apply(this, null, Observable.Return(true),
                        () => HeadingAnimationCompleted(null, null));
#endif
                }
            }
        }

        private void HeadingAnimationCompleted(object sender, object e)
        {
            if (headingAnimation != null)
            {
                SetValueInternal(HeadingProperty, TargetHeading);
                UpdateTransform();

#if !Avalonia
                headingAnimation.Completed -= HeadingAnimationCompleted;
                headingAnimation = null;

                this.BeginAnimation(HeadingProperty, null);
#endif
            }
        }

#if !Avalonia
        private void SetValueInternal(DependencyProperty property, object value)
#else
        private void SetValueInternal(AvaloniaProperty property, object value)
#endif
        {
            internalPropertyChange = true;

            SetValue(property, value);

            internalPropertyChange = false;
        }

        private void UpdateTransform(bool resetTransformCenter = false, bool projectionChanged = false)
        {
            var viewScale = ViewTransform.ZoomLevelToScale(ZoomLevel);
            var projection = MapProjection;

            projection.Center = ProjectionCenter ?? Center;

            var mapCenter = projection.LocationToMap(transformCenter ?? Center);

            if (MapProjection.IsValid(mapCenter))
            {
                ViewTransform.SetTransform(mapCenter, viewCenter, viewScale, Heading);

                if (transformCenter != null)
                {
#if !Avalonia
                    var center = ViewToLocation(new Point(RenderSize.Width / 2d, RenderSize.Height / 2d));
#else
                    var center = ViewToLocation(new Point(Bounds.Width / 2d, Bounds.Height / 2d));
#endif

                    if (center != null)
                    {
                        center.Longitude = Location.NormalizeLongitude(center.Longitude);

                        if (center.Latitude < -maxLatitude || center.Latitude > maxLatitude)
                        {
                            center.Latitude = Math.Min(Math.Max(center.Latitude, -maxLatitude), maxLatitude);
                            resetTransformCenter = true;
                        }

                        SetValueInternal(CenterProperty, center);

                        if (centerAnimation == null)
                        {
                            SetValueInternal(TargetCenterProperty, center);
                        }

                        if (resetTransformCenter)
                        {
                            ResetTransformCenter();

                            projection.Center = ProjectionCenter ?? Center;

                            mapCenter = projection.LocationToMap(center);

                            if (MapProjection.IsValid(mapCenter))
                            {
                                ViewTransform.SetTransform(mapCenter, viewCenter, viewScale, Heading);
                            }
                        }
                    }
                }

                SetViewScale(ViewTransform.Scale);

                OnViewportChanged(new ViewportChangedEventArgs(projectionChanged, Center.Longitude - centerLongitude));

                centerLongitude = Center.Longitude;
            }
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            base.OnViewportChanged(e);

            ViewportChanged?.Invoke(this, e);
        }
    }
}
