# Light Crosshair Overlay

A small, free and lightweight transparent crosshair overlay for windowed games.

![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)
![.NET](https://img.shields.io/badge/.NET-10-blue.svg)
![Size](https://img.shields.io/badge/size-~270%20KB-lightgrey.svg)

**[Download CrosshairOverlay.exe](../../releases/latest)** · **[Website](https://lightcrosshairoverlay.github.io/CrosshairOverlay/)**

---

## What it is

Light Crosshair Overlay draws a small crosshair on top of your game, so you always have a
precise aiming point — even in games that do not show one.

It is a single `.exe`, about **270 KB**. No installer, no account, no network.
Double-click it and it runs.

## How it works

1. **Download** `CrosshairOverlay.exe` and run it.
2. A crosshair appears at the **center of your screen**.
3. Right-click the tray icon (or double-click it) to open **Settings** and customize it.
4. Your settings are saved automatically and restored at the next launch.

That is all. There is nothing else to configure.

## Advantages

- **Totally free** — no payment, no ads, no account, no trial. Open source (MIT).
- **Ultra light** — a single ~270 KB exe. No installer. No background service.
- **~0% CPU and RAM at rest** — the overlay only redraws when something changes.
  No polling, no threads, no network.
- **Safe and anti-cheat friendly** — no injection, no hooks, no game memory access,
  no driver, no admin rights. It is just a transparent window drawn on top of the
  screen, like the overlays of Discord or Steam. It does **not** read or modify any
  game.
- **Windowed and borderless games** — works on any windowed or borderless game.
  (It cannot draw over a fullscreen-exclusive game, by design.)
- **14 shapes** — dot, plus, cross, circle, circle + dot, circle + cross, square,
  diamond, triangle, T, plus + dot, plus + hole, horizontal line, vertical line.
- **4 languages** — French, English, Russian, Spanish (full interface, saved in settings).
- **Movable** — drag it with the mouse (with the "Move with mouse" checkbox),
  or type exact X/Y coordinates, or center it with one click.
- **Dark outline** — an optional dark outline keeps the crosshair visible on any
  background, bright or dark.

## Requirements

- Windows 10 or 11 (64-bit).
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
  (free, from Microsoft). If you already use Dex Tab Swift, you already have it.

## Safety, in detail

The source code is open, so anyone can verify this. The program:

- does **not** access the network (no `HttpClient`, no socket, no web request);
- does **not** hook the keyboard or mouse (no `SetWindowsHookEx`);
- does **not** read or write another process memory (no `ReadProcessMemory`,
  `WriteProcessMemory`, `OpenProcess`, `CreateRemoteThread`);
- does **not** inject code or send fake input (no `SendInput`, no `mouse_event`);
- does **not** install a driver and does **not** need administrator rights.

It draws a transparent window on top of the screen (`UpdateLayeredWindow`), like any legitimate overlay.

The only file it writes is its own settings file:
`%LOCALAPPDATA%\CrosshairOverlay\settings.json`.

## Build from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
# Generate the icon (needs Python 3 + Pillow)
python3 icons/make_ico_double_anneau.py

# Build
cd src
dotnet publish -c Release -o ../publish
```

The output is a single framework-dependent exe (`CrosshairOverlay.exe`).

## License

MIT — see [LICENSE](LICENSE). Free to use, copy, modify and share, including for
commercial use, as long as the license notice is kept.
