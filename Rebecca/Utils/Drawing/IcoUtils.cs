using Rebecca.Utils.Drawing.Icons;
using System.IO;
using System.Runtime.InteropServices;

namespace Rebecca.Utils.Drawing;

internal static class IcoUtils
{
    public static int[] DefaultEdges = { 16, 24, 32, 48, 64, 96, 128, 256 };

    public static byte[] CreateIco(
        Func<int, Bitmap> createBitmap, params int[] edges)
    {
        if (edges.Length == 0)
        {
            edges = DefaultEdges;
        }

        var numbers = (byte)edges.Length;

        var stream = new MemoryStream();
        var header = new IcoHeader
        {
            Reserved = 0,
            ImageType = ImageType.Icon,
            Numbers = numbers
        };

        stream.WriteStruct(header);

        var dataList = new List<byte[]>();

        var dataOffset = Marshal.SizeOf(typeof(IcoHeader)) + numbers * Marshal.SizeOf(typeof(IcoDirEntry));
        foreach (var edge in edges.OrderBy(x => x))
        {
            var limitedEdge = edge > 256 ? 256 : edge;
            var bytes = createBitmap(edge).ToBytes();
            dataList.Add(bytes);

            var dirEntry = new IcoDirEntry
            {
                Width = (byte)limitedEdge,
                Height = (byte)limitedEdge,
                NumberOfColors = 0,
                Reserved = 0,

                ColorPlanes = 0x100,
                BitsPerPixel = 0x20,

                DataSize = (uint)bytes.Length,
                DataOffset = (uint)dataOffset
            };

            stream.WriteStruct(dirEntry);

            dataOffset += bytes.Length;
        }

        foreach (var bytes in dataList)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        return stream.ToArray();
    }
}
