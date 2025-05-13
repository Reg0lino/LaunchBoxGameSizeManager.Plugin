# Game Size Manager Plugin for LaunchBox

**Author:** RegularRegs (reg0lino on GitHub)
**Current Version:** 1.0.0 

A LaunchBox plugin to calculate, display, and manage game disk size information, including fetching estimated required space from online sources.

---

## Features

*   Calculates local game installation/ROM sizes.
*   Fetches estimated required disk space via RAWG.io API.
*   Stores data in custom fields: `Game Size`, `Game Size Tier`, `Est. Required Space`, `Game Size Last Scanned`.
*   UI integration via Tools menu and Game Context menu. (Right Click)
*   Configurable scan options.
*   Error reporting. (txt files inside plugin folder)

---

## Installation & Usage (For Users)

**➡️ [Click here to go to the LATEST RELEASE for download and detailed instructions](https://github.com/reg0lino/LaunchBoxGameSizeManager.Plugin/releases)**


**Quick Install Steps (Detailed instructions are in the release download):**
1. Download the `.zip` file from the [Releases page](https://github.com/reg0lino/LaunchBoxGameSizeManager.Plugin/releases).
2. Right-click the zip folder, select Properties, and check UNBLOCK
3. Extract the `GameSizeManager` folder into your `LaunchBox\Plugins\` directory.
4. DOUBLE CHECK THE .DLL FILES AND MAKE SURE THEY ARE UNBLOCKED IN PROPERTIES (I CAN NOT STRESS THIS ENOUGH)
5. Create `RAWG_API_KEY.txt` in the `LaunchBox\Plugins\GameSizeManager\` folder with your RAWG.io API key.
6. Restart LaunchBox.

---

## For Developers / Building from Source

This section is for those who wish to build the plugin from its source code.

**Prerequisites:**
*   Visual Studio 2019 or newer (Community Edition is fine).
*   .NET Framework 4.7.2 Developer Pack.
*   A copy of `Unbroken.LaunchBox.Plugins.dll` (v13.21.0.0 or compatible) from your LaunchBox `Core` directory, placed into the `lib` folder of this project (or adjust project reference).
*   A copy of `Newtonsoft.Json.dll` (the version referenced by the project, obtainable via NuGet) available or ensure NuGet package restore is working.

**Build Steps:**
1. Clone this repository: `git clone https://github.com/reg0lino/LaunchBoxGameSizeManager.Plugin.git`
2. Open `LaunchBoxGameSizeManager.Plugin.sln` in Visual Studio.
3. Ensure the reference to `Unbroken.LaunchBox.Plugins.dll` is correct.
4. Restore NuGet packages (if `Newtonsoft.Json.dll` is managed via packages.config).
5. Select the desired build configuration (Debug or Release).
6. Build the solution (Build -> Build Solution).
7. The output DLL will be in `bin\[Configuration]\LaunchBoxGameSizeManager.Plugin.dll`.

**Project Structure:**
*   `/CorePlugin`: Main plugin logic.
*   `/Services`: Data access, file system operations, API communication.
*   `/UI`: Windows Forms dialogs.
*   `/Utils`: Helper classes and constants.
*   `/lib`: (Intended for LaunchBox API DLL - not committed to repo).

---

## Contributing

Currently, please report issues or suggest features via the [GitHub Issues page](https://github.com/reg0lino/LaunchBoxGameSizeManager.Plugin/issues).

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
