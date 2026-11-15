using System.Drawing;
using System.Text.Json;

namespace CrosshairOverlay;

/// <summary>
/// Crosshair shapes offered by the app.
/// WARNING: the order is part of the saved format (settings.json stores the
/// numeric value). Never reorder, only append at the end.
/// </summary>
public enum CrosshairShape
{
    Dot,
    Plus,
    Cross,
    Circle,
    CircleDot,
    CirclePlus,
    Square,
    Diamond,
    Triangle,
    Tee,
    PlusDot,
    PlusGap,
    LineH,
    LineV
}

/// <summary>
    /// User settings, saved as JSON in %LOCALAPPDATA%\CrosshairOverlay\settings.json.
    /// This file is the only file the app ever writes.
    /// </summary>
public sealed class AppSettings
{
    public CrosshairShape Shape { get; set; } = CrosshairShape.Plus;
    public int Size { get; set; } = 24;
    public int Thickness { get; set; } = 3;
    public string ColorHtml { get; set; } = "#00FF00";
    public bool DarkOutline { get; set; } = true;
    public int X { get; set; } = -1;
    public int Y { get; set; } = -1;
    public bool Visible { get; set; } = true;

    /// <summary>Interface language code: "fr", "en", "ru" or "es". Default French.</summary>
    public string Language { get; set; } = "fr";

    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CrosshairOverlay", "settings.json");

    public Color Color
    {
        get
        {
            try { return ColorTranslator.FromHtml(ColorHtml); }
            catch { return Color.Lime; }
        }
    }

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath));
                if (loaded is not null)
                {
                    return loaded.Clamped();
                }
            }
        }
        catch
        {
            // A corrupt settings file must never stop the app from starting.
        }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // Saving is best effort; the overlay keeps working without it.
        }
    }

    /// <summary>Center of the given screen, used when no position was saved yet.</summary>
    public void CenterOn(Rectangle screen)
    {
        X = screen.Left + screen.Width / 2;
        Y = screen.Top + screen.Height / 2;
    }

    /// <summary>Keep every value inside its valid range.</summary>
    public AppSettings Clamped()
    {
        Size = Math.Clamp(Size, 4, 80);
        Thickness = Math.Clamp(Thickness, 1, 10);
        if (!Enum.IsDefined(Shape))
        {
            Shape = CrosshairShape.Plus;
        }
        if (Language is not ("fr" or "en" or "ru" or "es"))
        {
            Language = "fr";
        }
        // The position: -1 means "not set yet, center on the screen".
        // Any real value must stay on the screen, otherwise a typed number
        // (or an appended digit) sends the crosshair off-screen and it vanishes.
        var screen = SystemInformation.VirtualScreen;
        if (X != -1)
        {
            X = Math.Clamp(X, screen.Left, screen.Right - 1);
        }
        if (Y != -1)
        {
            Y = Math.Clamp(Y, screen.Top, screen.Bottom - 1);
        }
        return this;
    }
}
