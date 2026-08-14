### StarTray Temperature
# Guide

StarTray shows your processor and graphics card temperatures directly in the Windows system tray. This guide covers everything the application can do and how to get to it.

### Contents

- [Getting started](#getting-started)
- [Themes](#themes)
  - [Changing a theme](#changing-a-theme)
  - [Creating your own theme](#creating-your-own-theme)
- [Hover display](#hover-display)
- [Showing and hiding icons](#showing-and-hiding-icons)
- [Temperature units](#temperature-units)
- [Run on startup](#run-on-startup)
- [Hardware information](#hardware-information)
- [Choosing which GPU to monitor](#choosing-which-gpu-to-monitor)
- [PawnIO driver](#pawnio-driver)
- [Double-clicking an icon ](#double-clicking-an-icon)
- [Exiting StarTray](#exiting-startray)
- [Where your settings are stored](#where-your-settings-are-stored)
- [Troubleshooting/FAQ](#troubleshootingfaq)

<br>

## Getting started

Launch StarTray and its icons appear in the system tray, one for your processor and one for your graphics card, each showing that component's current temperature.

If you cannot see them, click the upward arrow at the left edge of the system tray. Windows hides newly added tray icons there by default. To keep StarTray permanently visible, drag its icons out of that panel and onto the taskbar.

Reading hardware temperatures requires administrator rights, so StarTray asks for them when it starts. On the first launch it may also offer to install the PawnIO driver, which is needed for processor temperatures - see [PawnIO driver](#the-pawnio-driver).

Everything else is done from the right-click menu. Both icons open a menu of their own, and the two are almost identical. The difference is that a few settings apply only to the icon you opened the menu on: the theme and the hover display are set separately for the processor and the graphics card, so you can style and configure each one independently. The rest of the settings apply to the application as a whole, and it makes no difference which icon you use to reach them.

<br>

## Themes

A theme controls how a tray icon looks: the two colors of its gradient and the color of the temperature text drawn on top. StarTray comes with nine of them - **Light**, **Dark**, **11 Light**, **11 Dark**, **Blue**, **Green**, **Red**, **Obsidian** and **Sakura** - and you can create your own or adjust any of them to your preference.

Themes are set per icon, so your processor and graphics card icons can match or look completely different, whichever you prefer.

### Changing a theme

Right-click the icon you want to restyle, hover over **CPU theme** or **GPU theme**, and click a theme from the list. The icon changes immediately, and StarTray remembers the choice for next time.

### Creating your own theme

Custom themes are small text (json) files, and writing one takes a minute.

**1. Open the Themes folder.** Right-click either icon, hover over **CPU theme** or **GPU theme**, and click **Open Themes folder...**

**2. Copy one of the existing files and rename it.** Any of them works as a starting point. Give the copy a name no other theme is using, as this is what identifies your theme.

**3. Set the colors.** Open the file in any text editor. Each theme has four values:

| Field | What it controls |
| --- | --- |
| `DisplayName` | The name that appears in the theme menu. |
| `IconColor1` | The top color of the icon's gradient. |
| `IconColor2` | The bottom color of the icon's gradient. |
| `TextColor` | The color of the temperature text. |

A finished theme looks like this:

```json
{
  "DisplayName":"Obsidian",
  "IconColor1":"#AC00E6",
  "IconColor2":"#600080",
  "TextColor":"#BC47FF"
}
```

If you would rather have a single flat color than a gradient, give `IconColor1` and `IconColor2` the same value.

**4. Load it.** Right-click an icon, hover over **CPU theme** or **GPU theme**, and click **Reload themes**. Your theme now appears in the list alongside the built-in ones, ready to apply to either icon. Restarting StarTray does the same thing if you prefer.

<br>

## Hover display

Hovering the mouse over a tray icon brings up a small readout of that component's values. By default it only shows the temperature, but you can add more detail.

Right-click an icon, hover over **Hover display**, and tick whichever readings you want:

| Value | Processor | Graphics card |
| --- | --- | --- |
| 🌡️ Temperature | ✅ | ✅ |
| 🧠 Load | ✅ | ✅ |
| ⚡ Power usage | ✅ | ✅ |
| ⏱️ Clock speed | ✅ | ✅ |
| 💾 Memory usage | - | ✅ |

Each icon keeps its own selection, so you can show a full readout for your graphics card and just the temperature for your processor, for example. 

_Note: Readings your hardware does not report are left out. Windows also limits how much text a tray tooltip can hold, so enabling everything at once may cut the readout short - pick the readings you actually want to watch._

<br>

## Showing and hiding icons

If you only care about one component, you can hide the other icon.

Right-click either icon, hover over **Options**, and click **Show CPU icon** or **Show GPU icon** to turn each one on or off. The checkmark shows which are currently visible.

One icon always stays visible, so you cannot hide both. If you are down to a single icon, turn the other one back on first.

<br>

## Temperature units

StarTray displays temperatures in Celsius by default and can switch to Fahrenheit.

Right-click either icon, hover over **Options**, and click **Change to Fahrenheit**. Both tray icons and both hover readouts switch over at once. The menu item always names the unit you would be switching to, so click **Change to Celsius** to go back.

<br>

## Run on startup

StarTray can start on its own whenever you sign in to Windows,

Right-click either icon, hover over **Options**, and click **Run on Startup**. A checkmark appears once it is enabled; click it again to turn it off. StarTray takes care of granting itself the administrator rights it needs at startup, so it will not prompt you every time you sign in.

<br>

## Hardware information

Right-click an icon and hover over **More** to see which hardware StarTray is reading. The processor menu shows the name of your processor, and the graphics card menu shows your graphics card.

<br>

## Choosing which GPU to monitor

Systems with more than one graphics card - a laptop with integrated and dedicated graphics, for instance - let you pick which one the GPU icon follows.

Right-click the GPU icon and hover over **More**. Every graphics card StarTray has detected is listed there, with a checkmark next to the one being monitored. Click a different card to switch to it. The icon starts reading from the new card straight away and stays on it from then on.

With only one graphics card installed, the entry is there for information and cannot be clicked.

<br>

## PawnIO driver

Reading processor temperatures on Windows requires a driver, and StarTray uses PawnIO, a third-party open-source one. You can read more about it at [pawnio.eu](https://pawnio.eu/).

If the driver is not already on your system, StarTray offers to install it when it starts. Click **Yes** and the installer runs, after which StarTray restarts itself and processor temperatures start working. If you decline, StarTray will not ask again.

You can install it at any point later: right-click either icon and click **Install PawnIO Driver**, near the top of the menu. That option is only shown while the driver is missing, so if you do not see it, PawnIO is already installed.

<br>

## Double-clicking an icon 

Double-click either tray icon to open Windows Task Manager - useful when a temperature spike has you wondering what is running.

<br>

## Exiting StarTray

Right-click either icon and click **Exit** at the bottom of the menu. This closes StarTray completely.

<br>

## Where your settings are stored

There is no save button anywhere in StarTray. Your themes, temperature unit, hover readings, visible icons, selected graphics card and startup setting are all saved the moment you change them and restored the next time the application runs.

Custom themes live in the `Themes` folder next to the application, the one that **Open Themes folder...** takes you to. If you have made themes you want to keep, back up that folder before reinstalling.

<br>

## Troubleshooting/FAQ

**The tray icons are nowhere to be seen.**  
Click the upward arrow at the left edge of the system tray - Windows hides new icons there. Drag StarTray out of that panel to keep it on the taskbar permanently.

**StarTray says it is already running.**  
Only one copy of StarTray runs at a time, and the one already running is most likely hidden in the panel behind the upward arrow.

**The processor icon shows no temperature.**  
This is almost always down to missing administrator rights or the PawnIO driver. Run StarTray as administrator, then check the right-click menu for **Install PawnIO Driver**.

**The graphics card icon is missing and the option is greyed out.**  
StarTray could not find a temperature sensor on your graphics card, so it hid the icon. Some integrated graphics do not report temperatures at all.

**A theme I made is not in the list.**  
Reopen the theme file and check that the text matches the example above - a missing comma or quotation mark is enough for StarTray to skip the file. Fix it and click **Reload themes**.

**Something else went wrong.**  
StarTray keeps a log of any problems it runs into at `%LocalAppData%\justinnas\StarTray\errors.log`. Including its contents when reporting an issue makes it far easier to track down.
