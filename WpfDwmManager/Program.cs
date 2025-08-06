using System;
using System.Threading;
using WpfDwmManager.Services;
using WpfDwmManager.Utils;

namespace WpfDwmManager
{
    public class Program
    {
        private static WindowManager? _windowManager;
        private static HotKeyService? _hotKeyService;
        private static WindowMonitoringService? _monitoringService;
        private static bool _isRunning = true;

        public static void Main(string[] args)
        {
            Logger.LogInfo("DWM Window Manager Starting...");
            Logger.ClearOldLogs(); // Clean up old log files
            
            try
            {
                InitializeServices();
                SetupConsoleUI();
                
                Logger.LogInfo("DWM Window Manager is running. Press 'q' to quit.");
                Console.WriteLine("DWM Window Manager is running. Press 'q' to quit.");
                RunMessageLoop();
            }
            catch (Exception ex)
            {
                Logger.LogError("Fatal error in main application", ex);
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            finally
            {
                Cleanup();
            }
        }

        private static void InitializeServices()
        {
            var config = ConfigManager.LoadConfiguration();
            Logger.LogInfo($"Configuration loaded from: {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}");
            
            _windowManager = new WindowManager();
            _hotKeyService = new HotKeyService(_windowManager);
            
            _windowManager.Initialize();
            _hotKeyService.RegisterHotKeys();
            
            // Start monitoring service
            _monitoringService = new WindowMonitoringService(_windowManager);
            
            var windowCount = _windowManager.GetManagedWindows().Count;
            Logger.LogInfo($"Window Manager initialized with {windowCount} windows");
            Console.WriteLine($"Window Manager initialized with {windowCount} windows");
        }

        private static void SetupConsoleUI()
        {
            Console.WriteLine();
            Console.WriteLine("=== DWM Window Manager ===");
            Console.WriteLine("Keyboard Shortcuts:");
            Console.WriteLine("  Win + Tab         - Switch between windows");
            Console.WriteLine("  Win + Shift + Enter - Set current window as master");
            Console.WriteLine("  Win + J/K         - Move focus up/down in stack");
            Console.WriteLine("  Win + H/L         - Resize master area");
            Console.WriteLine("  Win + T           - Toggle tiling mode");
            Console.WriteLine("  Win + Space       - Toggle floating/tiling");
            Console.WriteLine("  Win + F           - Toggle fullscreen");
            Console.WriteLine("  Win + Q           - Close current window");
            Console.WriteLine("  Win + Shift + C   - Exit window manager");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  'r' - Refresh window list");
            Console.WriteLine("  'l' - List managed windows");
            Console.WriteLine("  's' - Show current status");
            Console.WriteLine("  'p' - Pause/resume monitoring");
            Console.WriteLine("  'log' - Show log file path");
            Console.WriteLine("  'q' - Quit");
            Console.WriteLine();
        }

        private static void RunMessageLoop()
        {
            while (_isRunning)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    HandleConsoleInput(key);
                }
                
                Thread.Sleep(100);
            }
        }

        private static void HandleConsoleInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.L && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                // Handle Ctrl+L for "log" command
                ShowLogPath();
                return;
            }

            char key = char.ToLower(keyInfo.KeyChar);
            switch (key)
            {
                case 'q':
                    _isRunning = false;
                    Logger.LogInfo("User requested exit via console");
                    break;
                case 'r':
                    RefreshWindowList();
                    break;
                case 'l':
                    ListManagedWindows();
                    break;
                case 's':
                    ShowStatus();
                    break;
                case 'p':
                    ToggleMonitoring();
                    break;
                case 'h':
                    SetupConsoleUI();
                    break;
            }

            // Handle multi-character commands
            if (keyInfo.KeyChar == 'l' && Console.KeyAvailable)
            {
                var next1 = Console.ReadKey(true);
                var next2 = Console.ReadKey(true);
                if (next1.KeyChar == 'o' && next2.KeyChar == 'g')
                {
                    ShowLogPath();
                }
            }
        }

        private static void RefreshWindowList()
        {
            Logger.LogDebug("Manual window list refresh requested");
            _windowManager?.RefreshWindowList();
            var count = _windowManager?.GetManagedWindows().Count ?? 0;
            var message = $"Refreshed window list - found {count} managed windows";
            Logger.LogInfo(message);
            Console.WriteLine(message);
        }

        private static void ListManagedWindows()
        {
            var windows = _windowManager?.GetManagedWindows();
            if (windows == null || windows.Count == 0)
            {
                Console.WriteLine("No managed windows found");
                return;
            }

            Console.WriteLine($"Managed Windows ({windows.Count}):");
            for (int i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                var status = window.IsMaster ? " [MASTER]" : "";
                var floating = window.IsFloating ? " [FLOATING]" : "";
                var minimized = window.IsMinimized ? " [MINIMIZED]" : "";
                Console.WriteLine($"  {i + 1}. {window.Title} ({window.ProcessName}){status}{floating}{minimized}");
            }
        }

        private static void ShowStatus()
        {
            var currentLayout = _windowManager?.GetCurrentLayoutMode().ToString() ?? "Unknown";
            var windowCount = _windowManager?.GetManagedWindows().Count ?? 0;
            var focusedWindow = _windowManager?.GetFocusedWindow();
            
            Console.WriteLine($"Current Layout: {currentLayout}");
            Console.WriteLine($"Managed Windows: {windowCount}");
            Console.WriteLine($"Focused Window: {focusedWindow?.Title ?? "None"}");
            Console.WriteLine($"Monitoring: {(_monitoringService != null ? "Active" : "Inactive")}");
            Console.WriteLine($"Log Level: {ConfigManager.LoadConfiguration().LogLevel}");
        }

        private static void ToggleMonitoring()
        {
            if (_monitoringService != null)
            {
                _monitoringService.Stop();
                _monitoringService.Dispose();
                _monitoringService = null;
                Logger.LogInfo("Window monitoring stopped");
                Console.WriteLine("Window monitoring stopped");
            }
            else
            {
                if (_windowManager != null)
                {
                    _monitoringService = new WindowMonitoringService(_windowManager);
                    Logger.LogInfo("Window monitoring started");
                    Console.WriteLine("Window monitoring started");
                }
            }
        }

        private static void ShowLogPath()
        {
            var logPath = Logger.GetLogPath();
            Console.WriteLine($"Log file: {logPath}");
            Logger.LogDebug("Log path displayed to user");
        }

        private static void Cleanup()
        {
            Logger.LogInfo("Shutting down DWM Window Manager...");
            Console.WriteLine("Shutting down DWM Window Manager...");
            
            _monitoringService?.Dispose();
            _hotKeyService?.UnregisterHotKeys();
            _windowManager?.Dispose();
            _hotKeyService?.Dispose();
            
            Logger.LogInfo("DWM Window Manager shutdown complete");
        }
    }
}