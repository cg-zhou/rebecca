namespace Rebecca.Drawing;

/// <summary>
/// Polar coordinate system
/// </summary>
internal static class PcsUtils
{
    public static PointF Get(double r, double theta, PointF? center = null)
    {
        var x = Math.Cos(theta * Math.PI / 180) * r + (center?.X ?? 0);
        var y = Math.Sin(theta * Math.PI / 180) * r + (center?.Y ?? 0);

        return new PointF((float)x, (float)y);
    }
}
