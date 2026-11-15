using System.Drawing;

namespace CrosshairOverlay;

/// <summary>
/// The settings window. Small, movable, normal window.
/// Every change is applied immediately, saved to settings.json and redrawn on the overlay.
/// Closing the window only hides it; the app keeps running in the tray.
/// Dark theme (v1.3), shape picker with icons (v1.4), minimalist layout (v1.5),
/// four languages (v1.6), rounded interface like the mockup (v1.7): pill-shaped
/// shape buttons, blue sliders, round color swatches, white Close button.
/// </summary>
public sealed class SettingsForm : Form
{
    // Dark theme colors (mockup v1.7: no orange anymore, blue sliders, white Close).
    private static readonly Color DarkBackground = Color.FromArgb(32, 32, 32);
    private static readonly Color DarkControl = Color.FromArgb(45, 45, 45);
    private static readonly Color DarkButton = Color.FromArgb(58, 58, 58);
    private static readonly Color PillBackground = Color.FromArgb(51, 51, 51);
    private static readonly Color PillSelected = Color.FromArgb(74, 74, 74);
    private static readonly Color LightText = Color.FromArgb(232, 232, 232);
    private static readonly Color GreyText = Color.FromArgb(160, 160, 160);
    private static readonly Color SectionText = Color.FromArgb(138, 138, 138);
    private static readonly Color SwatchBorder = Color.FromArgb(85, 85, 85);

    // All shapes, in palette order (7 per row).
    private static readonly CrosshairShape[] ShapeOrder =
    {
        CrosshairShape.Dot, CrosshairShape.Plus, CrosshairShape.Cross,
        CrosshairShape.Circle, CrosshairShape.CircleDot, CrosshairShape.CirclePlus,
        CrosshairShape.Square, CrosshairShape.Diamond, CrosshairShape.Triangle,
        CrosshairShape.Tee, CrosshairShape.PlusDot, CrosshairShape.PlusGap,
        CrosshairShape.LineH, CrosshairShape.LineV
    };

    private readonly AppSettings _settings;
    private bool _syncing;

    private readonly PictureBox _preview = new() { Size = new Size(110, 110), BorderStyle = BorderStyle.FixedSingle };
    private readonly Label _shapeNameLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = LightText };
    private readonly Label _shapeInfoLabel = new() { AutoSize = false, Size = new Size(274, 36), Font = new Font("Segoe UI", 9f), ForeColor = GreyText };
    private readonly CheckBox _visibleBox = new() { AutoSize = true, ForeColor = LightText };
    private readonly Dictionary<CrosshairShape, RoundedButton> _shapeButtons = new();
    private readonly ToolTip _tips = new();
    private readonly Label _tailleMotLabel = new() { AutoSize = true };
    private readonly ColorSlider _sizeBar = new() { Minimum = 4, Maximum = 80, Width = 280, Height = 28 };
    private readonly Label _sizeValue = new() { AutoSize = true };
    private readonly Label _epaisseurMotLabel = new() { AutoSize = true };
    private readonly ColorSlider _thicknessBar = new() { Minimum = 1, Maximum = 10, Width = 280, Height = 28 };
    private readonly Label _thicknessValue = new() { AutoSize = true };
    private readonly List<RoundedButton> _colorButtons = new();
    private readonly CheckBox _outlineBox = new() { AutoSize = true, ForeColor = LightText };
    private readonly TextBox _posX = new() { Width = 90, BorderStyle = BorderStyle.None, BackColor = DarkControl, ForeColor = Color.White };
    private readonly TextBox _posY = new() { Width = 90, BorderStyle = BorderStyle.None, BackColor = DarkControl, ForeColor = Color.White };
    // The X/Y letters: fixed-size labels, text vertically centered, so they
    // share the exact baseline of the numbers in the fields (a Label drawn
    // from its top and a TextBox that centers its text never lined up).
    private readonly Label _xLabel = new() { Text = "X", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Label _yLabel = new() { Text = "Y", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
    private readonly CheckBox _moveModeBox = new() { AutoSize = true, ForeColor = LightText };
    private readonly Dictionary<string, RoundedButton> _languageButtons = new();

    // Section labels and buttons whose text changes with the language.
    private readonly Label _formeLabel;
    private readonly Label _reglagesLabel;
    private readonly Label _couleurLabel;
    private readonly Label _positionLabel;
    private readonly Label _langueLabel;
    private readonly RoundedButton _centerButton;
    private readonly RoundedButton _resetButton;
    private readonly RoundedButton _closeButton;

    /// <summary>Raised after any setting value changed (already written into _settings).</summary>
    public event Action? Changed;

    /// <summary>Raised when the user toggles "Déplacer à la souris".</summary>
    public event Action<bool>? MoveModeChanged;

    public SettingsForm(AppSettings settings)
    {
        _settings = settings;
        Text = "Crosshair Overlay";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = DarkBackground;
        ForeColor = LightText;
        // The height is set at the end of the constructor from the last
        // control, so no dead space is left under the buttons (bug v1.0).
        KeyPreview = true;

        // Header row: preview on the left, current shape on the right.
        _preview.Location = new Point(12, 16);
        Controls.Add(_preview);
        _shapeNameLabel.Location = new Point(134, 20);
        Controls.Add(_shapeNameLabel);
        // The summary, on one line when it fits (like the mockup), on two
        // lines when it does not, so it is never cut off. The label is tall
        // enough for two lines (36 px); the checkbox sits below it.
        _shapeInfoLabel.Location = new Point(134, 48);
        Controls.Add(_shapeInfoLabel);
        _visibleBox.Location = new Point(134, 88);
        Controls.Add(_visibleBox);

        var y = 138;

        // Shape picker: pill-shaped buttons with a drawn icon. Only the
        // selected one gets a lighter tile and a white border.
        _formeLabel = SectionLabel(y);
        Controls.Add(_formeLabel);
        y += 18;
        for (int i = 0; i < ShapeOrder.Length; i++)
        {
            var shape = ShapeOrder[i];
            var button = new RoundedButton
            {
                Size = new Size(52, 40),
                Location = new Point(12 + (i % 7) * 57, y + (i / 7) * 46),
                CornerRadius = 12,
                BackColor = PillBackground,
                Image = CrosshairRenderer.RenderShapeIcon(shape, 52, 40),
                ImageAlign = ContentAlignment.MiddleCenter,
                Tag = shape
            };
            button.Click += (_, _) => { _settings.Shape = shape; OnChanged(); };
            _shapeButtons[shape] = button;
            Controls.Add(button);
        }
        y += 86 + 16;

        // Sliders, grouped under RÉGLAGES like the mockup, with a label on
        // the left of each one and the value on the right.
        _reglagesLabel = SectionLabel(y);
        Controls.Add(_reglagesLabel);
        y += 18;
        _tailleMotLabel.Location = new Point(12, y + 4);
        Controls.Add(_tailleMotLabel);
        _sizeBar.Location = new Point(80, y);
        _sizeValue.Location = new Point(366, y + 4);
        Controls.Add(_sizeBar);
        Controls.Add(_sizeValue);
        y += 34;
        _epaisseurMotLabel.Location = new Point(12, y + 4);
        Controls.Add(_epaisseurMotLabel);
        _thicknessBar.Location = new Point(80, y);
        _thicknessValue.Location = new Point(366, y + 4);
        Controls.Add(_thicknessBar);
        Controls.Add(_thicknessValue);
        y += 28 + 16;

        // Colors: round swatches, plus a "+" circle for a custom color.
        _couleurLabel = SectionLabel(y);
        Controls.Add(_couleurLabel);
        y += 18;
        string[] presetColors = { "#FFFFFF", "#000000", "#FF0000", "#00FF00", "#FFFF00", "#00FFFF", "#FF00FF" };
        for (int i = 0; i < presetColors.Length; i++)
        {
            string html = presetColors[i];
            var button = new RoundedButton
            {
                Size = new Size(36, 36),
                Location = new Point(12 + i * 44, y),
                CornerRadius = 18,
                BackColor = ColorTranslator.FromHtml(html),
                Tag = html
            };
            button.Click += (_, _) => { _settings.ColorHtml = html; OnChanged(); };
            _colorButtons.Add(button);
            Controls.Add(button);
        }
        var plusButton = new RoundedButton
        {
            Size = new Size(36, 36),
            Location = new Point(12 + presetColors.Length * 44, y),
            CornerRadius = 18,
            BackColor = PillBackground,
            ForeColor = Color.White,
            // The "+" is drawn as two bars, not as text: a text "+" is
            // centered on its line box, which sits ~2 px above the visual
            // center of the glyph, so it always looked off-center.
            Image = RenderPlusIcon(16, 3, Color.White)
        };
        _tips.SetToolTip(plusButton, Strings.Get(_settings.Language, "Personnalisee"));
        plusButton.Click += (_, _) =>
        {
            using var dialog = new ColorDialog { Color = _settings.Color, FullOpen = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _settings.ColorHtml = ColorTranslator.ToHtml(dialog.Color);
                OnChanged();
            }
        };
        Controls.Add(plusButton);
        y += 36 + 10;
        _outlineBox.Location = new Point(12, y);
        Controls.Add(_outlineBox);
        y += 23 + 16;

        // Position: rounded fields (no spin buttons, like the mockup).
        _positionLabel = SectionLabel(y);
        Controls.Add(_positionLabel);
        y += 18;
        Controls.Add(_xLabel);
        // A single-line TextBox sizes its own height to the font (~23 px);
        // it is centered in the 26 px row. Its rounded corners are applied
        // in Shown, once the height has settled: applying the region earlier
        // used the 26 px asked height, so the bottom corners came out square.
        _posX.Location = new Point(30, y + 1);
        _posX.Size = new Size(90, 26);
        Controls.Add(_posX);
        Controls.Add(_yLabel);
        _posY.Location = new Point(150, y + 1);
        _posY.Size = new Size(90, 26);
        Controls.Add(_posY);
        _centerButton = new RoundedButton
        {
            Location = new Point(280, y),
            Size = new Size(116, 26),
            CornerRadius = 8,
            BackColor = DarkButton,
            ForeColor = Color.White
        };
        _centerButton.Click += (_, _) =>
        {
            _settings.CenterOn(PrimaryScreenBounds());
            OnChanged();
        };
        Controls.Add(_centerButton);
        y += 26 + 10;
        _moveModeBox.Location = new Point(12, y);
        Controls.Add(_moveModeBox);
        y += 23 + 12;

        // Language picker: one pill per language, named in itself.
        _langueLabel = SectionLabel(y);
        Controls.Add(_langueLabel);
        y += 18;
        foreach (var (code, index) in Strings.All.Select((code, index) => (code, index)))
        {
            var button = new RoundedButton
            {
                Text = Strings.LanguageName(code),
                Size = new Size(78, 26),
                Location = new Point(12 + index * 86, y),
                CornerRadius = 13,
                BackColor = DarkBackground,
                ForeColor = Color.White,
                Tag = code
            };
            button.Click += (_, _) =>
            {
                _settings.Language = code;
                ApplyLanguage();
                OnChanged();
            };
            _languageButtons[code] = button;
            Controls.Add(button);
        }
        y += 26 + 15;

        _resetButton = new RoundedButton
        {
            Location = new Point(12, y),
            Size = new Size(110, 30),
            CornerRadius = 8,
            BackColor = DarkButton,
            ForeColor = Color.White
        };
        _resetButton.Click += (_, _) =>
        {
            var fresh = new AppSettings();
            fresh.CenterOn(PrimaryScreenBounds());
            _settings.Shape = fresh.Shape;
            _settings.Size = fresh.Size;
            _settings.Thickness = fresh.Thickness;
            _settings.ColorHtml = fresh.ColorHtml;
            _settings.DarkOutline = fresh.DarkOutline;
            _settings.X = fresh.X;
            _settings.Y = fresh.Y;
            _settings.Visible = fresh.Visible;
            OnChanged();
        };
        Controls.Add(_resetButton);
        _closeButton = new RoundedButton
        {
            Location = new Point(286, y),
            Size = new Size(110, 30),
            CornerRadius = 8,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 30, 30)
        };
        _closeButton.Click += (_, _) => Hide();
        Controls.Add(_closeButton);

        _visibleBox.CheckedChanged += (_, _) => { _settings.Visible = _visibleBox.Checked; OnChanged(); };
        _sizeBar.ValueChanged += (_, _) => { _settings.Size = _sizeBar.Value; OnChanged(); };
        _thicknessBar.ValueChanged += (_, _) => { _settings.Thickness = _thicknessBar.Value; OnChanged(); };
        _outlineBox.CheckedChanged += (_, _) => { _settings.DarkOutline = _outlineBox.Checked; OnChanged(); };
        _posX.TextChanged += (_, _) =>
        {
            if (int.TryParse(_posX.Text, out int v))
            {
                _settings.X = v;
                OnChanged();
            }
        };
        _posY.TextChanged += (_, _) =>
        {
            if (int.TryParse(_posY.Text, out int v))
            {
                _settings.Y = v;
                OnChanged();
            }
        };
        // While the user is typing a number, a half-typed or empty value
        // must not be reverted by the refresh; it is re-synced on leave.
        _posX.Leave += (_, _) => { _posX.Text = _settings.X.ToString(); };
        _posY.Leave += (_, _) => { _posY.Text = _settings.Y.ToString(); };
        // Clicking into a position field selects its whole number, so typing
        // replaces it instead of appending a digit (960 -> 9605 sent the
        // crosshair off-screen). The value is committed live, keystroke by
        // keystroke, and clamped to the screen by AppSettings.Clamped.
        _posX.Enter += (_, _) => _posX.SelectAll();
        _posY.Enter += (_, _) => _posY.SelectAll();
        _posX.KeyDown += (_, e) => StepPosition(e, v => { _settings.X = v; });
        _posY.KeyDown += (_, e) => StepPosition(e, v => { _settings.Y = v; });
        _moveModeBox.CheckedChanged += (_, _) =>
        {
            if (_syncing)
            {
                return;
            }
            MoveModeChanged?.Invoke(_moveModeBox.Checked);
        };

        KeyDown += (_, e) =>
        {
            // A slider or a position box already uses the arrows for itself.
            // Moving the crosshair as well applied the key twice (bug v1.0).
            if (ActiveControl is ColorSlider or TextBox)
            {
                return;
            }
            int step = e.Shift ? 10 : 1;
            switch (e.KeyCode)
            {
                case Keys.Left: _settings.X -= step; OnChanged(); e.Handled = true; e.SuppressKeyPress = true; break;
                case Keys.Right: _settings.X += step; OnChanged(); e.Handled = true; e.SuppressKeyPress = true; break;
                case Keys.Up: _settings.Y -= step; OnChanged(); e.Handled = true; e.SuppressKeyPress = true; break;
                case Keys.Down: _settings.Y += step; OnChanged(); e.Handled = true; e.SuppressKeyPress = true; break;
            }
        };

        FormClosing += (_, e) =>
        {
            // Never dispose: the tray icon reopens this same window.
            e.Cancel = true;
            Hide();
        };

        // No dead space under the buttons: the window ends 12 px below them.
        ClientSize = new Size(420, _closeButton.Bottom + 12);

        ApplyLanguage();
        RefreshFromSettings();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        NativeMethods.TryEnableDarkTitleBar(Handle);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // The X/Y fields have their final height now: round their corners,
        // and line up the X/Y letters with them (same top and height, text
        // vertically centered, so the letters share the numbers' baseline).
        UiHelpers.ApplyRoundedRegion(_posX, 8);
        UiHelpers.ApplyRoundedRegion(_posY, 8);
        _xLabel.Size = new Size(16, _posX.Height);
        _xLabel.Location = new Point(12, _posX.Top);
        _yLabel.Size = new Size(16, _posY.Height);
        _yLabel.Location = new Point(132, _posY.Top);
    }

    /// <summary>Renders a "+" icon of two centered bars (perfectly centered, unlike a text "+").</summary>
    private static Bitmap RenderPlusIcon(int size, int thickness, Color color)
    {
        var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(color);
        int c = size / 2;
        g.FillRectangle(brush, 0, c - thickness / 2, size, thickness);
        g.FillRectangle(brush, c - thickness / 2, 0, thickness, size);
        return bitmap;
    }

    /// <summary>Small grey caps label that starts a section (minimalist layout).</summary>
    private static Label SectionLabel(int y)
    {
        return new Label
        {
            Location = new Point(12, y),
            AutoSize = true,
            Font = new Font("Segoe UI", 8f),
            ForeColor = SectionText
        };
    }

    /// <summary>Bounds of the primary screen (where the game usually is).</summary>
    private static Rectangle PrimaryScreenBounds()
    {
        return Screen.PrimaryScreen?.Bounds ?? SystemInformation.VirtualScreen;
    }

    /// <summary>Up/Down in an X/Y field steps the value (Shift = 10) instead of doing nothing.</summary>
    private void StepPosition(KeyEventArgs e, Action<int> apply)
    {
        if (e.KeyCode is not (Keys.Up or Keys.Down))
        {
            return;
        }
        int step = e.Shift ? 10 : 1;
        var box = ActiveControl as TextBox;
        if (box is null)
        {
            return;
        }
        int current = box == _posX ? _settings.X : _settings.Y;
        int next = current + (e.KeyCode == Keys.Up ? -step : step);
        apply(next);
        // Show the stepped value right away (the refresh skips a focused
        // field on purpose, so it would otherwise keep the old text).
        _syncing = true;
        box.Text = next.ToString();
        box.SelectionStart = box.Text.Length;
        _syncing = false;
        OnChanged();
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    /// <summary>Sets every fixed text from the current language (section labels, buttons, checkboxes, tooltips).</summary>
    private void ApplyLanguage()
    {
        string lang = _settings.Language;
        _formeLabel.Text = Strings.Get(lang, "Forme");
        _reglagesLabel.Text = Strings.Get(lang, "ReglagesSection");
        _couleurLabel.Text = Strings.Get(lang, "Couleur");
        _positionLabel.Text = Strings.Get(lang, "Position");
        _langueLabel.Text = Strings.Get(lang, "Langue");
        _tailleMotLabel.Text = Strings.Get(lang, "TailleMot");
        _epaisseurMotLabel.Text = Strings.Get(lang, "EpaisseurMot");
        _visibleBox.Text = Strings.Get(lang, "Afficher");
        // The two option checkboxes are in capitals on the mockup.
        _outlineBox.Text = Strings.Get(lang, "ContourSombre").ToUpperInvariant();
        _moveModeBox.Text = Strings.Get(lang, "DeplacerSouris").ToUpperInvariant();
        _centerButton.Text = Strings.Get(lang, "Centrer");
        _resetButton.Text = Strings.Get(lang, "Reinitialiser");
        _closeButton.Text = Strings.Get(lang, "Fermer");
        foreach (var (shape, button) in _shapeButtons)
        {
            _tips.SetToolTip(button, Strings.ShapeName(lang, shape));
        }
        foreach (var (code, button) in _languageButtons)
        {
            bool selected = code == lang;
            button.BorderWidth = selected ? 2 : 0;
            button.BorderColor = Color.White;
            button.BackColor = selected ? DarkButton : DarkBackground;
        }
    }

    /// <summary>Syncs the move-mode checkbox from the outside (hotkey) without firing the event.</summary>
    public void SetMoveMode(bool moveMode)
    {
        _syncing = true;
        _moveModeBox.Checked = moveMode;
        _syncing = false;
    }

    /// <summary>Syncs every control from the settings (after a drag, a hotkey or a reset).</summary>
    public void RefreshFromSettings()
    {
        _syncing = true;
        string lang = _settings.Language;
        _shapeNameLabel.Text = Strings.ShapeName(lang, _settings.Shape);
        // The summary: color and size, then the thickness and the outline.
        // For a dot the thickness IS the dark ring, so "Contour 6" already
        // says it: repeating "Contour sombre" after it ("Contour 6 •
        // Contour sombre") was redundant, and the line ran past the edge
        // of the window and got cut off. Without the outline a dot has no
        // ring, so the thickness is not shown at all.
        string detail;
        if (_settings.Shape == CrosshairShape.Dot)
        {
            detail = _settings.DarkOutline
                ? $"{Strings.Get(lang, "Contour")} {_settings.Thickness}"
                : "";
        }
        else
        {
            string thicknessWord = Strings.Get(lang, "EpaisseurMot");
            detail = $"{thicknessWord} {_settings.Thickness}";
            if (_settings.DarkOutline)
            {
                detail += $" • {Strings.Get(lang, "ContourSombre")}";
            }
        }
        string head = $"{Strings.ColorName(lang, _settings.ColorHtml)} • {_settings.Size} px";
        string oneLine = detail == "" ? head : $"{head} • {detail}";
        // One line when it fits (like the mockup), two lines when it does
        // not, so nothing is ever cut off. A 4 px safety margin: the
        // measure can underestimate, and a wrapped line in a 32 px label
        // had its second line cut off.
        _shapeInfoLabel.Text = TextRenderer.MeasureText(oneLine, _shapeInfoLabel.Font).Width <= _shapeInfoLabel.Width - 4
            ? oneLine
            : detail == "" ? head : $"{head}\n{detail}";
        _visibleBox.Checked = _settings.Visible;
        foreach (var (shape, button) in _shapeButtons)
        {
            bool selected = shape == _settings.Shape;
            button.BorderWidth = selected ? 2 : 0;
            button.BorderColor = Color.White;
            button.BackColor = selected ? PillSelected : PillBackground;
        }
        _sizeBar.Value = _settings.Size;
        _sizeValue.Text = _settings.Size.ToString();
        _thicknessBar.Value = _settings.Thickness;
        _thicknessValue.Text = _settings.Thickness.ToString();
        // The thickness slider works for every shape now (for a dot it is the
        // outline width), so it is never greyed out.
        foreach (var button in _colorButtons)
        {
            bool selected = string.Equals((string?)button.Tag, _settings.ColorHtml, StringComparison.OrdinalIgnoreCase);
            // White ring when selected; a thin grey ring otherwise, so the
            // black swatch stays visible on the dark background.
            button.BorderWidth = selected ? 3 : 1;
            button.BorderColor = selected ? Color.White : SwatchBorder;
        }
        _outlineBox.Checked = _settings.DarkOutline;
        // Do not overwrite a field the user is typing in.
        if (!_posX.Focused)
        {
            _posX.Text = _settings.X.ToString();
        }
        if (!_posY.Focused)
        {
            _posY.Text = _settings.Y.ToString();
        }
        RefreshPreview();
        _syncing = false;
    }

    private void RefreshPreview()
    {
        var old = _preview.Image;
        _preview.Image = CrosshairRenderer.RenderPreview(_settings, _preview.Width, _preview.Height);
        old?.Dispose();
    }

    private void OnChanged()
    {
        if (_syncing)
        {
            return;
        }
        RefreshFromSettings();
        Changed?.Invoke();
    }
}
