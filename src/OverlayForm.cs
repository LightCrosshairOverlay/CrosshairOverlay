using System.Drawing;

namespace CrosshairOverlay;

/// <summary>
/// The overlay itself: a borderless, topmost, layered window covering the whole virtual screen.
/// It never takes focus, never shows in the taskbar, and lets every click pass through,
/// except while MoveMode is on (then dragging the window moves the crosshair).
/// Redrawn only when the settings or the position change.
/// </summary>
public sealed class OverlayForm : Form
{
    public const int HotkeyToggleVisible = 1;
    public const int HotkeyToggleMoveMode = 2;

    private readonly AppSettings _settings;
    private bool _moveMode;
    private bool _dragging;
    private Point _dragOffset;
    private long _lastRedrawMs;

    public event Action? PositionChanged;
    public event Action<int>? HotKeyPressed;

    /// <summary>True when both global hotkeys (Ctrl+H, Ctrl+J) were registered.</summary>
    public bool HotkeysRegistered { get; private set; }

    public OverlayForm(AppSettings settings)
    {
        _settings = settings;
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Bounds = SystemInformation.VirtualScreen;
        // BUG (arrière-plan) : si un écran est branché/débranché ou si la
        // résolution change pendant que l'app tourne, l'overlay gardait ses
        // anciennes dimensions et le crosshair finissait mal placé ou coupé.
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }
        base.Dispose(disposing);
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        if (!IsHandleCreated)
        {
            return;
        }
        try
        {
            // SystemEvents can fire on another thread: marshal to the UI thread.
            BeginInvoke(() =>
            {
                Bounds = SystemInformation.VirtualScreen;
                Redraw();
            });
        }
        catch (InvalidOperationException)
        {
            // The window is being destroyed: nothing left to update.
        }
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= NativeMethods.WS_EX_LAYERED
                | NativeMethods.WS_EX_TRANSPARENT
                | NativeMethods.WS_EX_TOOLWINDOW
                | NativeMethods.WS_EX_NOACTIVATE;
            return cp;
        }
    }

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool MoveMode
    {
        get => _moveMode;
        set
        {
            if (_moveMode == value)
            {
                return;
            }
            _moveMode = value;
            UpdateClickThrough();
            Cursor = value ? Cursors.SizeAll : Cursors.Default;
        }
    }

    private void UpdateClickThrough()
    {
        if (!IsHandleCreated)
        {
            return;
        }
        int style = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
        style = _moveMode
            ? style & ~NativeMethods.WS_EX_TRANSPARENT
            : style | NativeMethods.WS_EX_TRANSPARENT;
        NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE, style);
        // BUG v1.0 : SetWindowLong alone does NOT apply an extended-style change.
        // The window manager only re-reads the style after SetWindowPos with
        // SWP_FRAMECHANGED. Without this call the overlay stayed click-through,
        // so OnMouseDown never fired and dragging in move mode did nothing.
        NativeMethods.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_FRAMECHANGED);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // Ctrl+H: show/hide. Ctrl+J: move mode. RegisterHotKey only, no hooks.
        // BUG v1.0 : the return values were ignored, so a hotkey already taken
        // by another app failed silently. The result is exposed to the tray icon.
        bool showHide = NativeMethods.RegisterHotKey(Handle, HotkeyToggleVisible, 0x0002 /* MOD_CONTROL */, 0x48 /* H */);
        bool moveMode = NativeMethods.RegisterHotKey(Handle, HotkeyToggleMoveMode, 0x0002 /* MOD_CONTROL */, 0x4A /* J */);
        HotkeysRegistered = showHide && moveMode;
        UpdateClickThrough();
        Redraw();
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        NativeMethods.UnregisterHotKey(Handle, HotkeyToggleVisible);
        NativeMethods.UnregisterHotKey(Handle, HotkeyToggleMoveMode);
        base.OnHandleDestroyed(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_HOTKEY)
        {
            HotKeyPressed?.Invoke(m.WParam.ToInt32());
            return;
        }
        base.WndProc(ref m);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (_moveMode && e.Button == MouseButtons.Left)
        {
            _dragging = true;
            var screenPos = PointToScreen(e.Location);
            _dragOffset = new Point(_settings.X - screenPos.X, _settings.Y - screenPos.Y);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_moveMode && _dragging)
        {
            var screenPos = PointToScreen(e.Location);
            _settings.X = screenPos.X + _dragOffset.X;
            _settings.Y = screenPos.Y + _dragOffset.Y;
            ClampToVirtualScreen();
            // BUG (arrière-plan) : chaque mouvement de souris allouait une
            // image plein écran (~8 Mo) et la poussait à l'écran. À fort taux
            // de rapport souris, ça faisait des centaines de Mo/s et le glisser
            // saccadait. On redessine au maximum toutes les 16 ms (~60 ips),
            // et une dernière fois au relâchement.
            long now = Environment.TickCount64;
            if (now - _lastRedrawMs >= 16)
            {
                _lastRedrawMs = now;
                Redraw();
                PositionChanged?.Invoke();
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left && _dragging)
        {
            _dragging = false;
            Redraw();
            PositionChanged?.Invoke();
            _settings.Save();
        }
    }

    private void ClampToVirtualScreen()
    {
        var screen = SystemInformation.VirtualScreen;
        _settings.X = Math.Clamp(_settings.X, screen.Left, screen.Right - 1);
        _settings.Y = Math.Clamp(_settings.Y, screen.Top, screen.Bottom - 1);
    }

    /// <summary>Repaints the layered window from the current settings.</summary>
    public void Redraw()
    {
        if (!IsHandleCreated)
        {
            return;
        }

        var screen = SystemInformation.VirtualScreen;
        using var bitmap = CrosshairRenderer.Render(_settings, screen.Width, screen.Height, screen.Left, screen.Top);
        IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0, 0, 0, 0));
        IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
        IntPtr memDc = NativeMethods.CreateCompatibleDC(screenDc);
        IntPtr oldBitmap = NativeMethods.SelectObject(memDc, hBitmap);
        try
        {
            var pptDst = new NativeMethods.Point { X = screen.Left, Y = screen.Top };
            var psize = new NativeMethods.Size { Cx = screen.Width, Cy = screen.Height };
            var pprSrc = new NativeMethods.Point { X = 0, Y = 0 };
            var blend = new NativeMethods.BlendFunction
            {
                BlendOp = NativeMethods.AC_SRC_OVER,
                BlendFlags = 0,
                SourceConstantAlpha = 255,
                AlphaFormat = NativeMethods.AC_SRC_ALPHA
            };
            NativeMethods.UpdateLayeredWindow(Handle, screenDc, ref pptDst, ref psize,
                memDc, ref pprSrc, 0, ref blend, NativeMethods.ULW_ALPHA);
        }
        finally
        {
            NativeMethods.SelectObject(memDc, oldBitmap);
            NativeMethods.DeleteObject(hBitmap);
            NativeMethods.DeleteDC(memDc);
            NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
        }
    }
}
