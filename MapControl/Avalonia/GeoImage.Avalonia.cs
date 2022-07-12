using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class GeoImage
    {
         public static async Task<Tuple<Bitmap, Matrix>> ReadGeoTiff(string sourcePath)
        {
            return await Task.Run(() =>
            {
                Bitmap bitmap;
                Matrix transform;

                using (var stream = File.OpenRead(sourcePath))
                {
                    bitmap = new Bitmap(stream);
                }
                
                // TODO: Don't know how to do it properly
                //var metadata = (BitmapMetadata)bitmap.Metadata;
                
                //if (metadata.GetQuery(PixelScaleQuery) is double[] pixelScale && pixelScale.Length == 3 &&
                //    metadata.GetQuery(TiePointQuery) is double[] tiePoint && tiePoint.Length >= 6)
                //{
                //    transform = new Matrix(pixelScale[0], 0d, 0d, -pixelScale[1], tiePoint[3], tiePoint[4]);
                //}
                //else if (metadata.GetQuery(TransformQuery) is double[] tform && tform.Length == 16)
                //{
                //    transform = new Matrix(tform[0], tform[1], tform[4], tform[5], tform[3], tform[7]);
                //}
                //else
                //{
                //    throw new ArgumentException($"No coordinate transformation found in {sourcePath}.");
                //}

                //if (metadata.GetQuery(NoDataQuery) is string noData && int.TryParse(noData, out int noDataValue))
                //{
                //    bitmap = ConvertTransparentPixel(bitmap, noDataValue);
                //}
                transform = new Matrix();
                return new Tuple<Bitmap, Matrix>(bitmap, transform);
            });
        }
    }
}