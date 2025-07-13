using Rebecca.Drawing;
using System.Drawing.Drawing2D;

namespace Rebecca.Utils.Drawing;

internal class LogoUtils
{
    public static Color Blue = Color.FromArgb(0xFF, 0x00, 0x77, 0xDD);
    public static Color Yellow = Color.FromArgb(0xFF, 0xF7, 0xC2, 0x57);

    public static Bitmap DrawLogo(int edge, int angle = 0, Color? primaryColor = null)
    {
        var center = new Point(edge / 2, edge / 2);
        var bitmap = new Bitmap(edge, edge);

        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var whiteBrush = new SolidBrush(Color.White);

            primaryColor = primaryColor.HasValue ? primaryColor.Value : Blue;
            var primaryBrush = new SolidBrush(primaryColor.Value);
            var redBrush = new SolidBrush(Color.FromArgb(255, Color.Red));

            var bluePath = new GraphicsPath();
            foreach (var n in Enumerable.Range(0, 6))
            {
                var r = (int)(edge / 2.0 * 1.04);
                var theta = n * 60 + 30;
                var ratio = 0.3f;
                var p = PcsUtils.Get(r, theta, center);
                var p0 = PcsUtils.Get(r * (1 - ratio), theta, center);
                var r1 = r * ratio * (float)Math.Sin(60 * Math.PI / 180);

                var p1 = PcsUtils.Get(r1, n * 60, p0);
                var p2 = PcsUtils.Get(r1, (n + 1) * 60, p0);

                var rect = new RectangleF(p0.X - r1, p0.Y - r1, r1 * 2, r1 * 2);
                bluePath.AddArc(rect, 60 * n, 60);
            }

            bluePath.CloseAllFigures();
            g.FillPath(primaryBrush, bluePath);

            var whitePath = new GraphicsPath();
            var points = new List<PointF>();
            for (var i = 0; i < 3; i++)
            {
                var p0 = PcsUtils.Get(edge * 0.5, 120 * i - 30 + angle, center);
                var p1 = PcsUtils.Get(edge * 0.08, 120 * i + 30 + angle, center);
                points.Add(p0);
                points.Add(p1);
            }
            whitePath.AddPolygon(points.ToArray());
            whitePath.CloseAllFigures();
            g.FillPath(whiteBrush, whitePath);
        }

        return bitmap;
    }
}
