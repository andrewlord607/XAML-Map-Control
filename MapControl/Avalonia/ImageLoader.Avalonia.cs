using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class ImageLoader
    {
        public static Task<IImage> LoadImageAsync(Stream stream)
        {
            return Task.Run(() => LoadImage(stream));
        }

        public static Task<IImage> LoadImageAsync(byte[] buffer)
        {
            return Task.Run(() =>
            {
                using (var stream = new MemoryStream(buffer))
                {
                    return LoadImage(stream);
                }
            });
        }

        public static Task<IImage> LoadImageAsync(string path)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                using (var stream = File.OpenRead(path))
                {
                    return LoadImage(stream);
                }
            });
        }

        private static IImage LoadImage(Stream stream)
        {
            return new Bitmap(stream);
        }
    }
}