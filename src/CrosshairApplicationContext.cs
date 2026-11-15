using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrosshairOverlay;

/// <summary>
/// Owns the overlay, the settings window and the tray icon.
/// Hotkeys: Ctrl+H show/hide, Ctrl+J move mode (same keys as the reference app).
/// </summary>
public sealed class CrosshairApplicationContext : ApplicationContext
{
    private readonly AppSettings _settings;
    private readonly OverlayForm _overlay;
    private readonly SettingsForm _settingsForm;
    private readonly NotifyIcon _trayIcon;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly ToolStripMenuItem _settingsItem;
    private readonly ToolStripMenuItem _quitItem;
    private readonly System.Windows.Forms.Timer _saveTimer = new() { Interval = 400 };
    private IntPtr _trayIconHandle;

    public CrosshairApplicationContext(AppSettings settings)
    {
        _settings = settings;
        var virtualScreen = SystemInformation.VirtualScreen;
        // BUG (arrière-plan) : "Centrer" visait le milieu de l'écran virtuel
        // (tous les écrans réunis). Avec deux écrans côte à côte, ça tombait
        // sur la jointure entre les deux. On centre sur l'écran principal.
        var primaryScreen = Screen.PrimaryScreen?.Bounds ?? virtualScreen;
        if (_settings.X == -1 || _settings.Y == -1)
        {
            _settings.CenterOn(primaryScreen);
            _settings.Save();
        }
        // A monitor may have been unplugged since the position was saved.
        _settings.X = Math.Clamp(_settings.X, virtualScreen.Left, virtualScreen.Right - 1);
        _settings.Y = Math.Clamp(_settings.Y, virtualScreen.Top, virtualScreen.Bottom - 1);

        // BUG (arrière-plan) : chaque cran de curseur écrivait settings.json
        // (CreateDirectory + WriteAllText). On regroupe les écritures : une
        // sauvegarde au maximum toutes les 400 ms, plus une à la fermeture.
        _saveTimer.Tick += (_, _) => { _saveTimer.Stop(); _settings.Save(); };

        _settingsForm = new SettingsForm(_settings);
        _overlay = new OverlayForm(_settings);

        _settingsForm.Changed += ApplySettings;
        _settingsForm.MoveModeChanged += moveMode =>
        {
            _overlay.MoveMode = moveMode;
        };
        _overlay.PositionChanged += () =>
        {
            _settingsForm.RefreshFromSettings();
            // A dragged position must be saved too, not just shown.
            _saveTimer.Stop();
            _saveTimer.Start();
        };
        _overlay.HotKeyPressed += id =>
        {
            if (id == OverlayForm.HotkeyToggleVisible)
            {
                _settings.Visible = !_settings.Visible;
                ApplySettings();
            }
            else if (id == OverlayForm.HotkeyToggleMoveMode)
            {
                _overlay.MoveMode = !_overlay.MoveMode;
                _settingsForm.SetMoveMode(_overlay.MoveMode);
            }
        };

        _toggleItem = new ToolStripMenuItem(null, null, (_, _) =>
        {
            _settings.Visible = !_settings.Visible;
            ApplySettings();
        });
        _settingsItem = new ToolStripMenuItem(null, null, (_, _) => ShowSettings());
        _quitItem = new ToolStripMenuItem(null, null, (_, _) => Exit());
        var menu = new ContextMenuStrip();
        menu.Items.Add(_settingsItem);
        menu.Items.Add(_toggleItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_quitItem);

        _trayIcon = new NotifyIcon
        {
            Icon = BuildTrayIcon(),
            Text = "Crosshair Overlay",
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => ShowSettings();

        _overlay.Show();
        _settingsForm.Show();
        ApplySettings();
    }

    private void ShowSettings()
    {
        _settingsForm.Show();
        _settingsForm.Activate();
    }

    private void ApplySettings()
    {
        _settings.Clamped();
        _saveTimer.Stop();
        _saveTimer.Start();
        _overlay.Redraw();
        _settingsForm.RefreshFromSettings();
        // Tray texts follow the current language, like the settings window.
        string lang = _settings.Language;
        _toggleItem.Text = _settings.Visible ? Strings.Get(lang, "Masquer") : Strings.Get(lang, "Afficher");
        _settingsItem.Text = Strings.Get(lang, "Reglages");
        _quitItem.Text = Strings.Get(lang, "Quitter");
        _trayIcon.Text = _overlay.HotkeysRegistered
            ? "Crosshair Overlay"
            : Strings.Get(lang, "RaccourcisIndisponibles");
    }

    private void Exit()
    {
        _saveTimer.Stop();
        _settings.Save();
        _saveTimer.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        // BUG v1.0 : the HICON created by GetHicon was never destroyed.
        if (_trayIconHandle != IntPtr.Zero)
        {
            NativeMethods.DestroyIcon(_trayIconHandle);
            _trayIconHandle = IntPtr.Zero;
        }
        _overlay.Close();
        _settingsForm.Dispose();
        Application.Exit();
    }

    /// <summary>Double anneau tray icon, drawn at runtime: the same mark as the exe icon (two white rings and a dot on a dark tile), so the green "+" is gone. Crisp filled shapes, no thick-pen lines.</summary>
    private Icon BuildTrayIcon()
    {
        using var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            // Dark rounded tile, like the exe icon, so it reads on any taskbar.
            using (var tilePath = UiHelpers.RoundedPath(new Rectangle(0, 0, 32, 32), 7))
            using (var tileBrush = new SolidBrush(Color.FromArgb(32, 32, 32)))
            {
                g.FillPath(tileBrush, tilePath);
            }
            // Two white rings and a center dot (the Double anneau mark).
            using var ringPen = new Pen(Color.White, 2.5f);
            g.DrawEllipse(ringPen, 32 / 2 - 12, 32 / 2 - 12, 24, 24);
            g.DrawEllipse(ringPen, 32 / 2 - 7, 32 / 2 - 7, 14, 14);
            using var dotBrush = new SolidBrush(Color.White);
            g.FillEllipse(dotBrush, 32 / 2 - 2, 32 / 2 - 2, 4, 4);
        }
        _trayIconHandle = bitmap.GetHicon();
        return Icon.FromHandle(_trayIconHandle);
    }
}
