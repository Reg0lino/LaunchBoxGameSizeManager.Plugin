# LaunchBox Game Size and Cleanup Plugin - Project Mission 
 
"This project aims to create a LaunchBox plugin to help users manage their game library by:" 
"- Accurately calculating and displaying the disk space used by Windows game installations and ROM files." 
"- Allowing users to select a specific platform for scanning." 
"- Storing the calculated size in a custom LaunchBox game field for sorting and display." 
"- Providing options to manage games based on size, including:" 
"  - Opening game location in Explorer." 
"  - Safely removing game entries from LaunchBox (keeping files)." 
"  - Deleting game files and associated media (with strong warnings and user confirmation)." 
"  - Permanently deleting game entries, files, and all associated media (with strongest warnings)." 
"- All disk-intensive operations should be performed on background threads to keep LaunchBox responsive." 
"- The plugin should be developed using C# and the .NET Framework, targeting the LaunchBox Plugin API." 
 
## Project Structure 
 
- **/docs/**: Contains project documentation, design notes, etc. 
- **/lib/**: Will hold external libraries, specifically `Unbroken.LaunchBox.Plugins.dll`. 
- **/src/**: Contains all the source code for the plugin. 
  - **/src/CorePlugin/**: Main plugin class, entry points. 
  - **/src/Services/**: Business logic, LaunchBox API interaction, file system operations. 
  - **/src/UI/**: UI elements, dialogs, and UI management. 
  - **/src/Models/**: Data structures, enums (e.g., DeleteOperationType). 
  - **/src/Utils/**: Helper classes, common functions (e.g., size formatting). 
- **LaunchBoxGameSizeManager.sln**: (To be created by Visual Studio) The solution file. 
- **LaunchBoxGameSizeManager.csproj**: (To be created by Visual Studio within src/CorePlugin or a dedicated project folder) The C# project file. 
 
## Development Notes 
 
- **Target Framework:** .NET Framework (check LaunchBox current requirements, likely 4.6.2 or higher). 
- **Primary Language:** C# 
- **Key Dependency:** `Unbroken.LaunchBox.Plugins.dll` (from your LaunchBox `Core` directory). Place this in the `/lib/` folder. 
- **IDE:** Visual Studio (Community Edition is fine). 
- **Version Control:** Git (initialize a repository in the LaunchBoxGameSizeManager root). 
