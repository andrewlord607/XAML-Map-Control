// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#elif Avalonia
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Animation;
using Avalonia.Styling;
using System.Reactive.Linq;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays a single map image, e.g. from a Web Map Service (WMS).
    /// The image must be provided by the abstract GetImageAsync() method.
    /// </summary>
    public abstract class MapImageLayer : MapPanel, IMapLayer
    {
#if !Avalonia
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(MapImageLayer), new PropertyMetadata(null));
#else
        public static readonly AvaloniaProperty<string> DescriptionProperty = AvaloniaProperty.Register<MapImageLayer, string>(
            nameof(Description));
#endif

#if !Avalonia
        public static readonly DependencyProperty RelativeImageSizeProperty = DependencyProperty.Register(
            nameof(RelativeImageSize), typeof(double), typeof(MapImageLayer), new PropertyMetadata(1d));
#else
        public static readonly AvaloniaProperty<double> RelativeImageSizeProperty = AvaloniaProperty.Register<MapImageLayer, double>(
            nameof(RelativeImageSize), 1d);
#endif

#if !Avalonia
        public static readonly DependencyProperty UpdateIntervalProperty = DependencyProperty.Register(
            nameof(UpdateInterval), typeof(TimeSpan), typeof(MapImageLayer),
            new PropertyMetadata(TimeSpan.FromSeconds(0.2), (o, e) => ((MapImageLayer)o).updateTimer.Interval = (TimeSpan)e.NewValue));
#else
        public static readonly AvaloniaProperty<TimeSpan> UpdateIntervalProperty = AvaloniaProperty.Register<MapImageLayer, TimeSpan>(
            nameof(UpdateInterval), TimeSpan.FromSeconds(0.2));
#endif

#if !Avalonia
        public static readonly DependencyProperty UpdateWhileViewportChangingProperty = DependencyProperty.Register(
            nameof(UpdateWhileViewportChanging), typeof(bool), typeof(MapImageLayer), new PropertyMetadata(false));
#else
        public static readonly AvaloniaProperty<bool> UpdateWhileViewportChangingProperty = AvaloniaProperty.Register<MapImageLayer, bool>(
            nameof(UpdateWhileViewportChanging), false);
#endif

#if !Avalonia
        public static readonly DependencyProperty MapBackgroundProperty = DependencyProperty.Register(
            nameof(MapBackground), typeof(Brush), typeof(MapImageLayer), new PropertyMetadata(null));
#else
        public static readonly StyledProperty<IBrush> MapBackgroundProperty = AvaloniaProperty.Register<MapImageLayer, IBrush>(
            nameof(MapBackground));
#endif

#if !Avalonia
        public static readonly DependencyProperty MapForegroundProperty = DependencyProperty.Register(
            nameof(MapForeground), typeof(Brush), typeof(MapImageLayer), new PropertyMetadata(null));
#else
        public static readonly StyledProperty<IBrush> MapForegroundProperty = AvaloniaProperty.Register<MapImageLayer, IBrush>(
            nameof(MapForeground));
#endif

#if WINUI
        private readonly DispatcherQueueTimer updateTimer;
#else
        private readonly DispatcherTimer updateTimer;
#endif
        private bool updateInProgress;

#if Avalonia
        static MapImageLayer()
        {
            UpdateIntervalProperty.Changed.AddClassHandler<MapImageLayer>((o, e) => o.updateTimer.Interval = (TimeSpan)e.NewValue);
        }
#endif

        public MapImageLayer()
        {
            updateTimer = this.CreateTimer(UpdateInterval);
            updateTimer.Tick += async (s, e) => await UpdateImageAsync();
        }

        /// <summary>
        /// Description of the layer. Used to display copyright information on top of the map.
        /// </summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        /// <summary>
        /// Relative size of the map image in relation to the current view size.
        /// Setting a value greater than one will let MapImageLayer request images that
        /// are larger than the view, in order to support smooth panning.
        /// </summary>
        public double RelativeImageSize
        {
            get { return (double)GetValue(RelativeImageSizeProperty); }
            set { SetValue(RelativeImageSizeProperty, value); }
        }

        /// <summary>
        /// Minimum time interval between images updates.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get { return (TimeSpan)GetValue(UpdateIntervalProperty); }
            set { SetValue(UpdateIntervalProperty, value); }
        }

        /// <summary>
        /// Controls if images are updated while the viewport is still changing.
        /// </summary>
        public bool UpdateWhileViewportChanging
        {
            get { return (bool)GetValue(UpdateWhileViewportChangingProperty); }
            set { SetValue(UpdateWhileViewportChangingProperty, value); }
        }

        /// <summary>
        /// Optional background brush. Sets MapBase.Background if not null and this layer is the base map layer.
        /// </summary>
#if !Avalonia
        public Brush MapBackground
#else
        public IBrush MapBackground
#endif
        {
#if !Avalonia
            get { return (Brush)GetValue(MapBackgroundProperty); }
#else
            get { return (Brush)GetValue(MapBackgroundProperty); }
#endif
            set { SetValue(MapBackgroundProperty, value); }
        }

        /// <summary>
        /// Optional foreground brush. Sets MapBase.Foreground if not null and this layer is the base map layer.
        /// </summary>
#if !Avalonia
        public Brush MapForeground
#else
        public IBrush MapForeground
#endif
        {
#if !Avalonia
            get { return (Brush)GetValue(MapForegroundProperty); }
#else
            get { return GetValue(MapForegroundProperty); }
#endif
            set { SetValue(MapForegroundProperty, value); }
        }

        /// <summary>
        /// The current BoundingBox
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

#if !Avalonia
        protected abstract Task<ImageSource> GetImageAsync();
#else
        protected abstract Task<IImage> GetImageAsync();
#endif

        protected override void SetParentMap(MapBase map)
        {
            if (map == null)
            {
                updateTimer.Stop();
                ClearImages();
                Children.Clear();
            }
            else if (Children.Count == 0)
            {
                Children.Add(new Image { Opacity = 0d, Stretch = Stretch.Fill });
                Children.Add(new Image { Opacity = 0d, Stretch = Stretch.Fill });
            }

            base.SetParentMap(map);
        }

        protected override async void OnViewportChanged(ViewportChangedEventArgs e)
        {
            if (e.ProjectionChanged)
            {
                ClearImages();

                base.OnViewportChanged(e);

                await UpdateImageAsync();
            }
            else
            {
                AdjustBoundingBox(e.LongitudeOffset);

                base.OnViewportChanged(e);

                updateTimer.Run(!UpdateWhileViewportChanging);
            }
        }

        protected async Task UpdateImageAsync()
        {
            updateTimer.Stop();

            if (updateInProgress)
            {
                updateTimer.Run(); // update image on next timer tick
            }
#if !Avalonia
            else if (ParentMap != null && ParentMap.RenderSize.Width > 0 && ParentMap.RenderSize.Height > 0)
#else
            else if (ParentMap != null && ParentMap.Bounds.Width > 0 && ParentMap.Bounds.Height > 0)
#endif
            {
                updateInProgress = true;

                UpdateBoundingBox();

#if !Avalonia
                ImageSource image = null;
#else
                IImage image = null;
#endif

                if (BoundingBox != null)
                {
                    try
                    {
                        image = await GetImageAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"MapImageLayer: {ex.Message}");
                    }
                }

                SwapImages(image);

                updateInProgress = false;
            }
        }

        private void UpdateBoundingBox()
        {
#if !Avalonia
            var width = ParentMap.RenderSize.Width * RelativeImageSize;
            var height = ParentMap.RenderSize.Height * RelativeImageSize;
            var x = (ParentMap.RenderSize.Width - width) / 2d;
            var y = (ParentMap.RenderSize.Height - height) / 2d;
#else
            var width = ParentMap.Bounds.Width * RelativeImageSize;
            var height = ParentMap.Bounds.Height * RelativeImageSize;
            var x = (ParentMap.Bounds.Width - width) / 2d;
            var y = (ParentMap.Bounds.Height - height) / 2d;
#endif
            var rect = new Rect(x, y, width, height);

            BoundingBox = ParentMap.ViewRectToBoundingBox(rect);
        }

        private void AdjustBoundingBox(double longitudeOffset)
        {
            if (Math.Abs(longitudeOffset) > 180d && BoundingBox != null)
            {
                var offset = 360d * Math.Sign(longitudeOffset);

                BoundingBox = new BoundingBox(BoundingBox, offset);

                foreach (var image in Children.OfType<Image>())
                {
                    var imageBoundingBox = GetBoundingBox(image);

                    if (imageBoundingBox != null)
                    {
                        SetBoundingBox(image, new BoundingBox(imageBoundingBox, offset));
                    }
                }
            }
        }

        private void ClearImages()
        {
            foreach (var image in Children.OfType<Image>())
            {
                image.ClearValue(BoundingBoxProperty);
                image.ClearValue(Image.SourceProperty);
            }
        }

#if !Avalonia
        private void SwapImages(ImageSource image)
#else
        private void SwapImages(IImage image)
#endif
        {
            if (Children.Count >= 2)
            {
                var topImage = (Image)Children[0];
                var bottomImage = (Image)Children[1];

                Children.RemoveAt(0);
                Children.Insert(1, topImage);

                topImage.Source = image;
                SetBoundingBox(topImage, BoundingBox);

#if !Avalonia
                topImage.BeginAnimation(OpacityProperty, new DoubleAnimation
                {
                    To = 1d,
                    Duration = MapBase.ImageFadeDuration
                });

                bottomImage.BeginAnimation(OpacityProperty, new DoubleAnimation
                {
                    To = 0d,
                    BeginTime = MapBase.ImageFadeDuration,
                    Duration = TimeSpan.Zero
                });
#else
                var topAnimation = new Animation
                {
                    Duration = MapBase.ImageFadeDuration,
                    Children =
                    {
                        new KeyFrame
                        {
                            KeyTime = MapBase.ImageFadeDuration,
                            Setters = { new Setter(OpacityProperty, 1d) }
                        }
                    }
                };
                topAnimation.Apply(topImage, null, Observable.Return(true), () =>
                {
                    var bottomAnimation = new Animation()
                    {
                        Duration = TimeSpan.Zero,
                        Children =
                        {
                            new KeyFrame()
                            {
                                KeyTime = TimeSpan.Zero,
                                Setters = { new Setter(OpacityProperty, 0d) }
                            }
                        }
                    };
                    bottomAnimation.RunAsync(bottomImage, null);
                });
#endif
            }
        }
    }
}
