using System.Threading;

namespace CrosshairOverlay;

/// <summary>
/// Crosshair Overlay 1.8.2 (final) — a small transparent crosshair overlay for windowed games.
/// Safe by design: no network, no admin rights, no hooks, no injection, no game memory access.
/// It only draws a transparent window on top of the screen and saves its own settings.json.
/// </summary>
internal static class Program
{
    // Held for the whole lifetime of the app: a second launch finds the
    // mutex already taken and exits instead of stacking a second overlay.
    private static Mutex? _instanceMutex;

    [STAThread]
    private static void Main()
    {
        var settings = AppSettings.Load();

        _instanceMutex = new Mutex(true, "CrosshairOverlay-SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(Strings.Get(settings.Language, "DejaEnCours"),
                "Crosshair Overlay", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        // BUG (arrière-plan) : sans ça, une erreur inattendue sur le thread
        // d'interface (par ex. GDI+ dans le redessin) fermait l'app sans rien
        // dire. Maintenant elle s'affiche au lieu d'un plantage silencieux.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
        {
            MessageBox.Show(Strings.Get(settings.Language, "ErreurInattendue") + e.Exception.Message,
                "Crosshair Overlay", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        Application.Run(new CrosshairApplicationContext(settings));
    }
}
