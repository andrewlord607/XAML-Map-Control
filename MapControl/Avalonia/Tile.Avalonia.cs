using Avalonia.Media;
using System;

namespace MapControl
{
    public partial class Tile
    {
        public void SetImage(IImage image, bool fadeIn = true)
        {
            Pending = false;

            if (image != null && fadeIn && MapBase.ImageFadeDuration > TimeSpan.Zero)
            {
                FadeIn();
            }
            else
            {
                Image.Opacity = 1d;
            }

            Image.Source = image;
        }
    }
}