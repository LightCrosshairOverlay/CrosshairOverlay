using System.Drawing.Drawing2D;

namespace CrosshairOverlay;

/// <summary>Small drawing helpers for the rounded interface (v1.7).</summary>
public static class UiHelpers
{
    /// <summary>Rounded-rectangle path. The radius is clamped to half the smallest side.</summary>
    public static GraphicsPath RoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return path;
        }
        int r = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
        if (r == 0)
        {
            path.AddRectangle(rect);
            return path;
        }
        int d = r * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>Clips a control to a rounded rectangle (used for the X/Y text boxes).</summary>
    public static void ApplyRoundedRegion(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            return;
        }
        using var path = RoundedPath(new Rectangle(0, 0, control.Width, control.Height), radius);
        control.Region = new Region(path);
    }
}
