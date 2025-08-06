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
        private static bool _isRunning = true;

        public static void Main(string[] args)
        {
            Console.WriteLine("DWM Window Manager Starting...");
            
            try
            {
                InitializeServices();
                SetupConsoleUI();
                
                Console.WriteLine("DWM Window Manager is running. Press 'q' to quit.");
                RunMessageLoop();
            }
            catch (Exception ex)
            {
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
            Console.WriteLine($"Configuration loaded from: {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}");
            
            _windowManager = new WindowManager();
            _hotKeyService = new HotKeyService(_windowManager);
            
            _windowManager.Initialize();
            _hotKeyService.RegisterHotKeys();
            
            Console.WriteLine($"Window Manager initialized with {_windowManager.GetManagedWindows().Count} windows");
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
                    HandleConsoleInput(key.KeyChar);
                }
                
                Thread.Sleep(100);
            }
        }

        private static void HandleConsoleInput(char key)
        {
            switch (char.ToLower(key))
            {
                case 'q':
                    _isRunning = false;
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
                case 'h':
                    SetupConsoleUI();
                    break;
            }
        }

        private static void RefreshWindowList()
        {
            _windowManager?.RefreshWindowList();
            var count = _windowManager?.GetManagedWindows().Count ?? 0;
            Console.WriteLine($"Refreshed window list - found {count} managed windows");
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
                Console.WriteLine($"  {i + 1}. {window.Title} ({window.ProcessName}){status}{floating}");
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
        }

        private static void Cleanup()
        {
            Console.WriteLine("Shutting down DWM Window Manager...");
            
            _hotKeyService?.UnregisterHotKeys();
            _windowManager?.Dispose();
            _hotKeyService?.Dispose();
        }
    }
}