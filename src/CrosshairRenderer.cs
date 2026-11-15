using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CrosshairOverlay;

/// <summary>
/// Draws the crosshair into a transparent bitmap.
/// Only called when the settings or the position change, so the overlay costs nothing while idle.
/// </summary>
public static class CrosshairRenderer
{
    private static readonly Color OutlineColor = Color.FromArgb(255, 20, 20, 20);

    /// <summary>
    /// Renders the crosshair for a screen-sized bitmap.
    /// <paramref name="originX"/> / <paramref name="originY"/> are the top-left corner of the
    /// virtual screen; the settings X/Y are virtual-screen coordinates.
    /// </summary>
    public static Bitmap Render(AppSettings settings, int width, int height, int originX, int originY)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        if (settings.Visible)
        {
            DrawCrosshair(g, settings.Shape, settings.X - originX, settings.Y - originY,
                settings.Size, settings.Thickness, settings.Color, settings.DarkOutline);
        }

        return bitmap;
    }

    /// <summary>Small preview (used by the settings window) on a dark background with a subtle grid.</summary>
    public static Bitmap RenderPreview(AppSettings settings, int width, int height)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(255, 40, 40, 40));

        // Subtle grid, like graph paper: it makes the shape easier to judge.
        using var gridPen = new Pen(Color.FromArgb(255, 56, 56, 56));
        for (int x = 10; x < width; x += 10)
        {
            g.DrawLine(gridPen, x, 0, x, height);
        }
        for (int y = 10; y < height; y += 10)
        {
            g.DrawLine(gridPen, 0, y, width, y);
        }

        int size = Math.Min(settings.Size, Math.Min(width, height) - 8);
        DrawCrosshair(g, settings.Shape, width / 2, height / 2,
            size, settings.Thickness, settings.Color, settings.DarkOutline);

        return bitmap;
    }

    /// <summary>
    /// Small white icon of one shape, for the shape picker buttons.
    /// Always white (readable on the dark buttons) with a dark outline.
    /// </summary>
    public static Bitmap RenderShapeIcon(CrosshairShape shape, int width, int height)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        int size = Math.Min(width, height) - 10;
        DrawCrosshair(g, shape, width / 2, height / 2, size, 3, Color.White, darkOutline: true);

        return bitmap;
    }

    /// <summary>
    /// Draws one shape. <paramref name="size"/> is the overall extent (diameter,
    /// side, bar length). <paramref name="thickness"/> is the width of the shape's
    /// own stroke (bar width, ring width, line width). For a dot the size is the
    /// diameter and the thickness is the width of the dark ring around it.
    /// <paramref name="darkOutline"/> adds a dark stroke 2 px wider on each side
    /// underneath, so the shape stays readable on any background.
    /// </summary>
    private static void DrawCrosshair(Graphics g, CrosshairShape shape, int cx, int cy,
        int size, int thickness, Color color, bool darkOutline)
    {
        // Extra half-width of the dark stroke underneath (2 px on each side).
        int outline = darkOutline ? 2 : 0;

        switch (shape)
        {
            case CrosshairShape.Dot:
                DrawDot(g, cx, cy, size, color, darkOutline ? thickness : 0);
                break;
            case CrosshairShape.Plus:
                DrawBars(g, new[]
                {
                    (new Point(cx - size / 2, cy), new Point(cx + size / 2, cy)),
                    (new Point(cx, cy - size / 2), new Point(cx, cy + size / 2))
                }, thickness, color, darkOutline);
                break;
            case CrosshairShape.Cross:
                DrawBars(g, new[]
                {
                    (new Point(cx - size / 2, cy - size / 2), new Point(cx + size / 2, cy + size / 2)),
                    (new Point(cx + size / 2, cy - size / 2), new Point(cx - size / 2, cy + size / 2))
                }, thickness, color, darkOutline);
                break;
            case CrosshairShape.Circle:
                DrawRing(g, cx, cy, size, thickness, color, outline);
                break;
            case CrosshairShape.CircleDot:
                DrawRing(g, cx, cy, size, thickness, color, outline);
                DrawDot(g, cx, cy, Math.Max(4, size / 4), color, outline);
                break;
            case CrosshairShape.CirclePlus:
                DrawRing(g, cx, cy, size, thickness, color, outline);
                int inner = (int)(size * 0.35);
                DrawBars(g, new[]
                {
                    (new Point(cx - inner, cy), new Point(cx + inner, cy)),
                    (new Point(cx, cy - inner), new Point(cx, cy + inner))
                }, thickness, color, darkOutline);
                break;
            case CrosshairShape.Square:
                DrawRect(g, cx, cy, size, thickness, color, outline);
                break;
            case CrosshairShape.Diamond:
                DrawPolygon(g, new[]
                {
                    new Point(cx, cy - size / 2),
                    new Point(cx + size / 2, cy),
                    new Point(cx, cy + size / 2),
                    new Point(cx - size / 2, cy)
                }, thickness, color, outline);
                break;
            case CrosshairShape.Triangle:
                DrawPolygon(g, new[]
                {
                    new Point(cx, cy - size / 2),
                    new Point(cx + size / 2, cy + size / 2),
                    new Point(cx - size / 2, cy + size / 2)
                }, thickness, color, outline);
                break;
            case CrosshairShape.Tee:
                DrawBars(g, new[]
                {
                    (new Point(cx - size / 2, cy - size / 4), new Point(cx + size / 2, cy - size / 4)),
                    (new Point(cx, cy - size / 4), new Point(cx, cy + size / 2))
                }, thickness, color, darkOutline);
                break;
            case CrosshairShape.PlusDot:
                DrawBars(g, new[]
                {
                    (new Point(cx - size / 2, cy), new Point(cx + size / 2, cy)),
                    (new Point(cx, cy - size / 2), new Point(cx, cy + size / 2))
                }, thickness, color, darkOutline);
                DrawDot(g, cx, cy, Math.Max(4, size / 4), color, outline);
                break;
            case CrosshairShape.PlusGap:
                // The gap must stay visible even at icon size (40 px): a
                // third of the size, at least 8 px. The old quarter-size gap
                // was too small to see and the shape read as a plain plus.
                int gap = Math.Max(8, size / 3);
                int half = size / 2;
                DrawBars(g, new[]
                {
                    (new Point(cx - half, cy), new Point(cx - gap / 2, cy)),
                    (new Point(cx + gap / 2, cy), new Point(cx + half, cy)),
                    (new Point(cx, cy - half), new Point(cx, cy - gap / 2)),
                    (new Point(cx, cy + gap / 2), new Point(cx, cy + half))
                }, thickness, color, darkOutline);
                break;
            case CrosshairShape.LineH:
                DrawBars(g, new[]
                {
                    (new Point(cx - size / 2, cy), new Point(cx + size / 2, cy))
                }, thickness, color, darkOutline);
                break;
            case CrosshairShape.LineV:
                DrawBars(g, new[]
                {
                    (new Point(cx, cy - size / 2), new Point(cx, cy + size / 2))
                }, thickness, color, darkOutline);
                break;
        }
    }

    private static void DrawDot(Graphics g, int cx, int cy, int diameter, Color color, int outlinePad)
    {
        if (outlinePad > 0)
        {
            using var outline = new SolidBrush(OutlineColor);
            g.FillEllipse(outline, cx - diameter / 2 - outlinePad, cy - diameter / 2 - outlinePad,
                diameter + outlinePad * 2, diameter + outlinePad * 2);
        }
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, cx - diameter / 2, cy - diameter / 2, diameter, diameter);
    }

    /// <summary>
    /// Draws a set of bars (Plus, Cross, Tee, lines, ...).
    /// BUG (contour sombre) : drawing each bar's dark outline immediately
    /// before its own color painted a dark band across the bars drawn
    /// before it, at every junction (the T's stem cut a dark notch in its
    /// own bar, the Plus had a dark cross in its middle). Now ALL the dark
    /// outlines are drawn first, then ALL the colored bars, so no dark
    /// stroke ever paints over a colored one. The dark stroke is also
    /// extended 2 px past each end so the border wraps the tips, which
    /// flat caps alone do not do.
    /// </summary>
    private static void DrawBars(Graphics g, (Point A, Point B)[] segments, int thickness, Color color, bool darkOutline)
    {
        if (darkOutline)
        {
            using var dark = new Pen(OutlineColor, thickness + 4)
            {
                StartCap = LineCap.Flat,
                EndCap = LineCap.Flat
            };
            foreach (var (a, b) in segments)
            {
                var (a2, b2) = Extend(a, b, 2);
                g.DrawLine(dark, a2, b2);
            }
        }
        using var pen = new Pen(color, thickness)
        {
            StartCap = LineCap.Flat,
            EndCap = LineCap.Flat
        };
        foreach (var (a, b) in segments)
        {
            g.DrawLine(pen, a, b);
        }
    }

    /// <summary>Extends a segment by <paramref name="by"/> px past each end, along its direction.</summary>
    private static (Point, Point) Extend(Point a, Point b, int by)
    {
        float dx = b.X - a.X, dy = b.Y - a.Y;
        float len = (float)Math.Sqrt(dx * dx + dy * dy);
        if (len < 0.001f)
        {
            return (a, b);
        }
        float ux = dx / len, uy = dy / len;
        return (new Point((int)Math.Round(a.X - ux * by), (int)Math.Round(a.Y - uy * by)),
                new Point((int)Math.Round(b.X + ux * by), (int)Math.Round(b.Y + uy * by)));
    }

    /// <summary>Draws a ring (circle outline), with an optional dark outline underneath.</summary>
    private static void DrawRing(Graphics g, int cx, int cy, int diameter, int thickness, Color color, int outlinePad)
    {
        if (outlinePad > 0)
        {
            using var outlinePen = new Pen(OutlineColor, thickness + outlinePad * 2);
            g.DrawEllipse(outlinePen, cx - diameter / 2, cy - diameter / 2, diameter, diameter);
        }
        using var pen = new Pen(color, thickness);
        g.DrawEllipse(pen, cx - diameter / 2, cy - diameter / 2, diameter, diameter);
    }

    /// <summary>Draws a square outline, with an optional dark outline underneath.</summary>
    private static void DrawRect(Graphics g, int cx, int cy, int side, int thickness, Color color, int outlinePad)
    {
        if (outlinePad > 0)
        {
            using var outlinePen = new Pen(OutlineColor, thickness + outlinePad * 2);
            g.DrawRectangle(outlinePen, cx - side / 2, cy - side / 2, side, side);
        }
        using var pen = new Pen(color, thickness);
        g.DrawRectangle(pen, cx - side / 2, cy - side / 2, side, side);
    }

    /// <summary>Draws a polygon outline (diamond, triangle), with an optional dark outline underneath.</summary>
    private static void DrawPolygon(Graphics g, Point[] points, int thickness, Color color, int outlinePad)
    {
        if (outlinePad > 0)
        {
            using var outlinePen = new Pen(OutlineColor, thickness + outlinePad * 2) { LineJoin = LineJoin.Round };
            g.DrawPolygon(outlinePen, points);
        }
        using var pen = new Pen(color, thickness) { LineJoin = LineJoin.Round };
        g.DrawPolygon(pen, points);
    }
}
