# WPF DWM Window Manager

A complete implementation of a dynamic window manager (DWM) for Windows using C# and .NET, inspired by the Linux dwm window manager. This project provides tiling window management with keyboard shortcuts, multi-monitor support, and configurable layouts.

## Features

### ✅ Core Window Management
- **Window Detection**: Automatically detects and manages application windows
- **Window Filtering**: Configurable rules to exclude system windows
- **Real-time Updates**: Monitors window creation, destruction, and focus changes
- **Cross-platform Compatibility**: Works on Windows (with native APIs) and Linux (with mock implementations)

### ✅ Layout Engine
- **Master-Stack Layout**: One master window with stack on the side
- **Grid Layout**: Windows arranged in a configurable grid
- **Floating Mode**: Windows can be moved and resized freely
- **Fullscreen Mode**: Focus window takes entire screen
- **Dynamic Switching**: Change layouts on-the-fly with hotkeys

### ✅ Keyboard Shortcuts
- `Win + Tab` - Switch between windows
- `Win + Shift + Enter` - Set current window as master
- `Win + J/K` - Move focus up/down in stack
- `Win + H/L` - Resize master area
- `Win + T` - Toggle layout modes
- `Win + Space` - Toggle floating/tiling
- `Win + F` - Toggle fullscreen
- `Win + Q` - Close current window
- `Win + Shift + C` - Exit window manager

### ✅ Multi-Monitor Support
- Automatic display detection
- Independent window management per monitor
- Move windows between monitors with `Win + Shift + Arrow`

### ✅ Configuration System
- JSON-based configuration
- Customizable hotkeys
- Layout parameters (master ratio, gaps, etc.)
- Window filtering rules
- Auto-saves to `%APPDATA%/WpfDwmManager/config.json`

## Project Structure

```
WpfDwmManager/
├── WpfDwmManager.csproj    # Project file
├── Program.cs              # Main application entry point
├── Models/
│   ├── WindowInfo.cs       # Window information model
│   ├── LayoutMode.cs       # Layout modes and settings
│   └── Configuration.cs    # Configuration model
├── Services/
│   ├── WindowManager.cs    # Core window management
│   ├── LayoutEngine.cs     # Window layout algorithms
│   ├── HotKeyService.cs    # Global hotkey handling
│   └── DisplayManager.cs   # Multi-monitor support
├── Utils/
│   ├── WinApi.cs           # Windows API wrapper
│   └── ConfigManager.cs    # Configuration management
└── Resources/
    └── config.json         # Default configuration
```

## Getting Started

### Prerequisites
- .NET 8.0 or higher
- Windows 10/11 (recommended) or Linux (for development/testing)

### Building
```bash
cd WpfDwmManager
dotnet build
```

### Running
```bash
cd WpfDwmManager
dotnet run
```

### Console Commands
When running, the following interactive commands are available:
- `l` - List managed windows
- `s` - Show current status
- `r` - Refresh window list
- `h` - Show help
- `q` - Quit application

## Configuration

The application creates a default configuration file at:
- Windows: `%APPDATA%\WpfDwmManager\config.json`
- Linux: `~/.config/WpfDwmManager/config.json`

### Sample Configuration
```json
{
  "Layout": {
    "MasterRatio": 0.55,
    "WindowGap": 5,
    "BorderWidth": 2,
    "ShowBorders": true,
    "GridColumns": 2,
    "GridRows": 2
  },
  "HotKeys": {
    "SwitchWindows": "Win+Tab",
    "SetMaster": "Win+Shift+Return",
    "ToggleTiling": "Win+T"
  },
  "WindowFilterRules": [
    "dwm.exe",
    "explorer.exe",
    "taskmgr.exe"
  ],
  "EnableMultiMonitor": true
}
```

## Architecture

### WindowManager
- Enumerates system windows using Windows API
- Filters windows based on configuration rules
- Monitors window events (create, destroy, focus)
- Manages window collection and state

### LayoutEngine
- Calculates window positions and sizes
- Implements different layout algorithms
- Handles window gaps and borders
- Updates window positions via Windows API

### HotKeyService
- Registers global system hotkeys
- Maps key combinations to actions
- Handles hotkey events and dispatches commands

### Cross-Platform Support
The Windows API wrapper (`WinApi.cs`) includes:
- Native Windows API calls when running on Windows
- Mock implementations for development/testing on other platforms
- Runtime platform detection using `RuntimeInformation.IsOSPlatform()`

## Development Notes

This implementation demonstrates the complete architecture of a window manager including:

1. **Window Enumeration**: Using `EnumWindows` to discover application windows
2. **Window Positioning**: Using `SetWindowPos` to arrange windows
3. **Event Handling**: Using `SetWinEventHook` to monitor window changes
4. **Hotkey Registration**: Using `RegisterHotKey` for global shortcuts
5. **Multi-Monitor**: Using display APIs to handle multiple screens

The code is designed to be:
- **Modular**: Clear separation of concerns
- **Extensible**: Easy to add new layouts or features
- **Configurable**: All settings externalized to JSON
- **Robust**: Error handling and graceful degradation
- **Cross-platform**: Works for development on multiple OS

## Future Enhancements

- [ ] WPF-based system tray application
- [ ] Visual window border highlighting
- [ ] Window animation effects
- [ ] Custom layout scripting
- [ ] Workspace/tag system
- [ ] Auto-start with Windows
- [ ] GUI configuration editor

## License

This project is implemented as a demonstration of window manager architecture and dwm concepts for Windows platforms.