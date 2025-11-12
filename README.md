# Weather Controller

Weather Controller is a sample mod for Vintage Story that provides an in-game GUI for managing weather conditions.

## Join the Community

Stay in the loop with updates, share feedback, and report bugs directly on our Discord server:

 **[Weather Controller Discord Community](https://discord.gg/AnGTgqHA)** 

Hop in to discuss new releases, suggest improvements, and collaborate with other players.

## Features
- Open the weather controller with the default hotkey binding `O`.
- Select weather patterns, events, and wind profiles and apply them to the current region or all loaded regions.
- Configure temporal storms by picking their desired frequency (or turning them off entirely) directly from the dialog.
- Toggle automatic weather cycling on or off.
- Override the precipitation intensity or clear a previously set override.

Only players with the `controlserver` (or `root`) server privilege can apply changes.

## Installation

1. Build the project with Visual Studio or run `msbuild WeatherController.csproj`.
2. Copy the generated `WeatherController.dll` together with the included `modinfo.json` into a new folder inside `%APPDATA%/VintagestoryData/Mods`.
3. Restart Vintage Story to load the mod.
