// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
#if WINUI
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#elif UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#elif Avalonia
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
#else
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// The System.Net.Http.HttpClient instance used to download images via a http or https Uri.
        /// </summary>
        public static HttpClient HttpClient { get; set; } = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };


#if !Avalonia
        public static async Task<ImageSource> LoadImageAsync(Uri uri)
#else
        public static async Task<IImage> LoadImageAsync(Uri uri)
#endif
        {
#if !Avalonia
            ImageSource image = null;
#else
            IImage image = null;
#endif

            try
            {
                if (!uri.IsAbsoluteUri || uri.IsFile)
                {
                    image = await LoadImageAsync(uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString);
                }
                else if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    var response = await GetHttpResponseAsync(uri);

                    if (response != null && response.Buffer != null)
                    {
                        image = await LoadImageAsync(response.Buffer);
                    }
                }
                else
                {
#if !Avalonia
                    image = new BitmapImage(uri);
#else
                    if (!uri.OriginalString.StartsWith("avares://"))
                    {
                        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
                        uri = new Uri($"avares://{assemblyName}{uri.OriginalString}");
                    }

                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    var asset = assets.Open(uri);

                    image = new Bitmap(asset);
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageLoader: {uri}: {ex.Message}");
            }

            return image;
        }

        internal class HttpResponse
        {
            public byte[] Buffer { get; }
            public TimeSpan? MaxAge { get; }

            public HttpResponse(byte[] buffer, TimeSpan? maxAge)
            {
                Buffer = buffer;
                MaxAge = maxAge;
            }
        }

        internal static async Task<HttpResponse> GetHttpResponseAsync(Uri uri)
        {
            HttpResponse response = null;

            try
            {
                using (var responseMessage = await HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        byte[] buffer = null;

                        if (!responseMessage.Headers.TryGetValues("X-VE-Tile-Info", out IEnumerable<string> tileInfo) ||
                            !tileInfo.Contains("no-tile"))
                        {
                            buffer = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        }

                        response = new HttpResponse(buffer, responseMessage.Headers.CacheControl?.MaxAge);
                    }
                    else
                    {
                        Debug.WriteLine($"ImageLoader: {uri}: {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageLoader: {uri}: {ex.Message}");
            }

            return response;
        }
    }
}