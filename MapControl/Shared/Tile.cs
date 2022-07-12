﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#elif Avalonia
using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
#endif

namespace MapControl
{
    public partial class Tile
    {
        public Tile(int zoomLevel, int x, int y)
        {
            ZoomLevel = zoomLevel;
            X = x;
            Y = y;
        }

        public int ZoomLevel { get; }
        public int X { get; }
        public int Y { get; }

        public int XIndex
        {
            get
            {
                var numTiles = 1 << ZoomLevel;
                return ((X % numTiles) + numTiles) % numTiles;
            }
        }

        public Image Image { get; } = new Image { Opacity = 0d, Stretch = Stretch.Fill };

        public bool Pending { get; set; } = true;

        private void FadeIn()
        {
#if !Avalonia
            Image.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation
            {
                From = 0d,
                To = 1d,
                Duration = MapBase.ImageFadeDuration,
                FillBehavior = FillBehavior.Stop
            });
#else
            var imageAnimation = new Animation
            {
                Duration = MapBase.ImageFadeDuration,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.Zero,
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 0d
                            }
                        }
                    },
                    new KeyFrame
                    {
                        KeyTime = MapBase.ImageFadeDuration,
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 1d
                            }
                        }
                    }
                }
            };
            imageAnimation.RunAsync(Image, null);
#endif

            Image.Opacity = 1d;
        }
    }
}
