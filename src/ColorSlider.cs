using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CrosshairOverlay;

/// <summary>
/// A slider drawn by hand (WinForms' TrackBar cannot be tinted): a light track,
/// a blue filled part and a round blue thumb. Mouse drag, click-to-jump and
/// keyboard (arrows, PageUp/PageDown, Home/End) all work like a TrackBar.
/// </summary>
public sealed class ColorSlider : Control
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int Minimum { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int Maximum { get; set; } = 100;

    private int _value;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int Value
    {
        get => _value;
        set
        {
            int v = Math.Clamp(value, Minimum, Maximum);
            if (v == _value)
            {
                return;
            }
            _value = v;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ValueChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color FillColor { get; set; } = Color.FromArgb(46, 124, 230);
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color TrackColor { get; set; } = Color.FromArgb(220, 220, 220);
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int ThumbDiameter { get; set; } = 18;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int TrackHeight { get; set; } = 4;

    private bool _dragging;

    public ColorSlider()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
        TabStop = true;
        DoubleBuffered = true;
    }

    protected override bool IsInputKey(Keys keyData)
    {
        return keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down
            or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown
            || base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.Down: Value -= 1; e.Handled = true; break;
            case Keys.Right:
            case Keys.Up: Value += 1; e.Handled = true; break;
            case Keys.PageDown: Value -= 10; e.Handled = true; break;
            case Keys.PageUp: Value += 10; e.Handled = true; break;
            case Keys.Home: Value = Minimum; e.Handled = true; break;
            case Keys.End: Value = Maximum; e.Handled = true; break;
        }
    }

    private int ThumbRadius => ThumbDiameter / 2;

    private float ValueToX(int value)
    {
        if (Maximum == Minimum)
        {
            return ThumbRadius;
        }
        return ThumbRadius + (float)(value - Minimum) / (Maximum - Minimum) * (Width - 2 * ThumbRadius);
    }

    private int XToValue(float x)
    {
        float t = (x - ThumbRadius) / Math.Max(1, Width - 2 * ThumbRadius);
        return (int)Math.Round(Minimum + t * (Maximum - Minimum));
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }
        Focus();
        _dragging = true;
        Capture = true;
        Value = XToValue(e.X);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging)
        {
            Value = XToValue(e.X);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _dragging = false;
        Capture = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int cy = Height / 2;
        int x0 = ThumbRadius;
        int x1 = Width - ThumbRadius;
        int tx = (int)Math.Round(ValueToX(Value));

        // Track (light), full width.
        using (var trackPath = UiHelpers.RoundedPath(new Rectangle(x0, cy - TrackHeight / 2, x1 - x0, TrackHeight), TrackHeight / 2))
        using (var trackBrush = new SolidBrush(TrackColor))
        {
            g.FillPath(trackBrush, trackPath);
        }
        // Filled part (blue), from the left edge to the thumb.
        if (tx > x0)
        {
            using var fillPath = UiHelpers.RoundedPath(new Rectangle(x0, cy - TrackHeight / 2, tx - x0, TrackHeight), TrackHeight / 2);
            using var fillBrush = new SolidBrush(FillColor);
            g.FillPath(fillBrush, fillPath);
        }
        // Thumb: a blue circle.
        using var thumbBrush = new SolidBrush(FillColor);
        g.FillEllipse(thumbBrush, tx - ThumbRadius, cy - ThumbRadius, ThumbDiameter, ThumbDiameter);
    }
}
