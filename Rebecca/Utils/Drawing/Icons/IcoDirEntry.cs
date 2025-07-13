using System.Runtime.InteropServices;

namespace Rebecca.Utils.Drawing.Icons;

[StructLayout(LayoutKind.Sequential)]
internal struct IcoDirEntry
{
    public byte Width { get; set; }
    public byte Height { get; set; }
    public byte NumberOfColors { get; set; }
    public byte Reserved { get; set; }

    /// <summary>
    /// In ICO format: Specifies color planes. Should be 0 or 1.
    /// </summary>
    public ushort ColorPlanes { get; set; }
    /// <summary>
    /// In ICO format: Specifies bits per pixel.
    /// </summary>
    public ushort BitsPerPixel { get; set; }

    public uint DataSize { get; set; }
    public uint DataOffset { get; set; }
}
