using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rebecca.Utils.Drawing;

internal static class BitmapExtension
{
    public static Bitmap Resize(this Bitmap bitmap, int width, int height)
    {
        var result = new Bitmap(width, height);

        using (var g = Graphics.FromImage(result))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(bitmap, new Rectangle(0, 0, width, height));
        }

        return result;
    }

    public static Bitmap ToBitmap(this byte[] bytes)
    {
        using (var stream = new MemoryStream(bytes))
        {
            return new Bitmap(stream);
        }
    }

    public static byte[] ToBytes(this Image image)
    {
        using (var stream = new MemoryStream())
        {
            image.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
    }

    public static byte[] ToIco(this Bitmap bitmap, params int[] edges)
    {
        return IcoUtils.CreateIco(edge => bitmap.Resize(edge, edge), edges);
    }

    public static BitmapImage ToBitmapImage(this byte[] bytes)
    {
        return ToBitmapImage(bytes.ToStream());
    }

    public static BitmapImage ToBitmapImage(this Stream stream)
    {
        var iconBitmap = new BitmapImage();
        iconBitmap.BeginInit();
        iconBitmap.StreamSource = stream;
        iconBitmap.EndInit();
        return iconBitmap;
    }

    public static byte[] ToBytes(this ImageSource imageSource)
    {
        if (!(imageSource is BitmapSource bitmapSource))
        {
            throw new Exception("The image source is not bitmap image source");
        }

        using (var stream = new MemoryStream())
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(stream);
            return stream.ToArray();
        }
    }
}
