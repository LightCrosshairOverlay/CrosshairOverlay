using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CrosshairOverlay;

/// <summary>
/// A button with rounded corners, drawn by hand (WinForms has no rounded button).
/// Used for the shape pills, the round color swatches, the language pills and
/// the action buttons. The border is drawn inside the edge so the full width
/// stays visible; a soft white overlay marks hover and press.
///
/// No Region is set on purpose: clipping the control to the rounded path
/// cut the anti-aliased edge and left jagged corners (the "artefacts" on
/// the outlines). Instead the whole control is painted with the parent's
/// background color first, so the corners blend in and the rounded edge
/// stays smooth. The hit area is the full rectangle, which is unnoticeable.
/// </summary>
public sealed class RoundedButton : Button
{
    private int _cornerRadius = 8;
    private Color _borderColor = Color.Transparent;
    private int _borderWidth;
    private bool _hovered;
    private bool _pressed;

    /// <summary>Corner radius in px (half the height = a pill, half the size = a circle).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int BorderWidth
    {
        get => _borderWidth;
        set { _borderWidth = value; Invalidate(); }
    }

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _pressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _pressed = false;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Do nothing: OnPaint fills the parent color itself, so the corners
        // blend in and the rounded edge stays anti-aliased.
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        // Whole control first, in the parent's background color, so the
        // corners outside the rounded shape disappear into the form.
        Color bg = Parent?.BackColor ?? Color.FromArgb(32, 32, 32);
        using (var bgBrush = new SolidBrush(bg))
        {
            g.FillRectangle(bgBrush, ClientRectangle);
        }
        using var path = UiHelpers.RoundedPath(ClientRectangle, _cornerRadius);
        using (var brush = new SolidBrush(BackColor))
        {
            g.FillPath(brush, path);
        }
        if (_borderWidth > 0)
        {
            var borderRect = ClientRectangle;
            borderRect.Inflate(-_borderWidth / 2, -_borderWidth / 2);
            using var borderPath = UiHelpers.RoundedPath(borderRect, Math.Max(0, _cornerRadius - _borderWidth / 2));
            using var pen = new Pen(_borderColor, _borderWidth);
            g.DrawPath(pen, borderPath);
        }
        if (_hovered)
        {
            // Soft white veil: the button answers the mouse without changing color.
            using var veil = new SolidBrush(Color.FromArgb(_pressed ? 45 : 22, 255, 255, 255));
            g.FillPath(veil, path);
        }
        if (Image != null)
        {
            int x = (Width - Image.Width) / 2;
            int y = (Height - Image.Height) / 2;
            g.DrawImage(Image, x, y);
        }
        else if (!string.IsNullOrEmpty(Text))
        {
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
