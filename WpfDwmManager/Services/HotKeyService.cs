using System;
using System.Collections.Generic;
using System.Diagnostics;
using WpfDwmManager.Models;
using WpfDwmManager.Utils;

namespace WpfDwmManager.Services
{
    public class HotKeyService : IDisposable
    {
        private readonly WindowManager _windowManager;
        private readonly Dictionary<int, Action> _hotKeyActions = new Dictionary<int, Action>();
        private readonly Dictionary<string, (uint modifiers, uint key)> _keyMappings;
        private IntPtr _windowHandle;
        private int _hotKeyId = 1;

        public HotKeyService(WindowManager windowManager)
        {
            _windowManager = windowManager;
            _keyMappings = InitializeKeyMappings();
            _windowHandle = IntPtr.Zero; // Mock window handle for cross-platform compatibility
        }

        private Dictionary<string, (uint modifiers, uint key)> InitializeKeyMappings()
        {
            return new Dictionary<string, (uint, uint)>
            {
                ["Win+Tab"] = (WinApi.MOD_WIN, 9), // VK_TAB = 0x09
                ["Win+Shift+Return"] = (WinApi.MOD_WIN | WinApi.MOD_SHIFT, 13), // VK_RETURN = 0x0D
                ["Win+K"] = (WinApi.MOD_WIN, 75), // VK_K = 0x4B
                ["Win+J"] = (WinApi.MOD_WIN, 74), // VK_J = 0x4A
                ["Win+H"] = (WinApi.MOD_WIN, 72), // VK_H = 0x48
                ["Win+L"] = (WinApi.MOD_WIN, 76), // VK_L = 0x4C
                ["Win+T"] = (WinApi.MOD_WIN, 84), // VK_T = 0x54
                ["Win+Space"] = (WinApi.MOD_WIN, 32), // VK_SPACE = 0x20
                ["Win+F"] = (WinApi.MOD_WIN, 70), // VK_F = 0x46
                ["Win+Q"] = (WinApi.MOD_WIN, 81), // VK_Q = 0x51
                ["Win+Shift+C"] = (WinApi.MOD_WIN | WinApi.MOD_SHIFT, 67), // VK_C = 0x43
                ["Win+Shift+Right"] = (WinApi.MOD_WIN | WinApi.MOD_SHIFT, 39), // VK_RIGHT = 0x27
                ["Win+Shift+Left"] = (WinApi.MOD_WIN | WinApi.MOD_SHIFT, 37) // VK_LEFT = 0x25
            };
        }

        public void RegisterHotKeys()
        {
            var config = ConfigManager.LoadConfiguration();
            RegisterHotKey(config.HotKeys.SwitchWindows, () => _windowManager.SwitchToNextWindow());
            RegisterHotKey(config.HotKeys.SetMaster, () => SetCurrentWindowAsMaster());
            RegisterHotKey(config.HotKeys.FocusUp, () => FocusPreviousWindow());
            RegisterHotKey(config.HotKeys.FocusDown, () => FocusNextWindow());
            RegisterHotKey(config.HotKeys.ResizeLeft, () => ResizeMasterArea(-0.05));
            RegisterHotKey(config.HotKeys.ResizeRight, () => ResizeMasterArea(0.05));
            RegisterHotKey(config.HotKeys.ToggleTiling, () => ToggleLayout());
            RegisterHotKey(config.HotKeys.ToggleFloating, () => ToggleFloating());
            RegisterHotKey(config.HotKeys.ToggleFullscreen, () => ToggleFullscreen());
            RegisterHotKey(config.HotKeys.CloseWindow, () => CloseCurrentWindow());
            RegisterHotKey(config.HotKeys.ExitManager, () => ExitManager());
            RegisterHotKey(config.HotKeys.MoveToNextMonitor, () => MoveToNextMonitor());
            RegisterHotKey(config.HotKeys.MoveToPrevMonitor, () => MoveToPrevMonitor());

            Debug.WriteLine($"Registered {_hotKeyActions.Count} hotkeys");
        }

        private void RegisterHotKey(string keyCombo, Action action)
        {
            if (_keyMappings.TryGetValue(keyCombo, out var mapping))
            {
                int id = _hotKeyId++;
                if (WinApi.RegisterHotKey(_windowHandle, id, mapping.modifiers, mapping.key))
                {
                    _hotKeyActions[id] = action;
                    Debug.WriteLine($"Registered hotkey {keyCombo} with ID {id}");
                }
                else
                {
                    Debug.WriteLine($"Failed to register hotkey {keyCombo}");
                }
            }
        }

        public void ProcessHotKeyMessage(IntPtr wParam)
        {
            int hotKeyId = wParam.ToInt32();
            if (_hotKeyActions.TryGetValue(hotKeyId, out var action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error executing hotkey action: {ex.Message}");
                }
            }
        }

        #region Hotkey Actions

        private void SetCurrentWindowAsMaster()
        {
            var focusedWindow = _windowManager.GetFocusedWindow();
            if (focusedWindow != null)
            {
                _windowManager.SetMasterWindow(focusedWindow.Handle);
                Console.WriteLine($"Set {focusedWindow.Title} as master window");
            }
        }

        private void FocusNextWindow()
        {
            _windowManager.SwitchToNextWindow();
        }

        private void FocusPreviousWindow()
        {
            var windows = _windowManager.GetManagedWindows();
            if (windows.Count <= 1) return;

            var focusedWindow = _windowManager.GetFocusedWindow();
            int currentIndex = focusedWindow != null ? windows.IndexOf(focusedWindow) : 0;
            int prevIndex = currentIndex == 0 ? windows.Count - 1 : currentIndex - 1;

            _windowManager.FocusWindow(windows[prevIndex].Handle);
            Console.WriteLine($"Focused previous window: {windows[prevIndex].Title}");
        }

        private void ResizeMasterArea(double delta)
        {
            // This would need access to LayoutEngine - for now just log
            Console.WriteLine($"Resize master area by {delta:F2}");
        }

        private void ToggleLayout()
        {
            var currentLayout = _windowManager.GetCurrentLayoutMode();
            var nextLayout = currentLayout switch
            {
                LayoutMode.MasterStack => LayoutMode.Grid,
                LayoutMode.Grid => LayoutMode.MasterStack,
                _ => LayoutMode.MasterStack
            };
            _windowManager.SetLayoutMode(nextLayout);
            Console.WriteLine($"Switched layout to {nextLayout}");
        }

        private void ToggleFloating()
        {
            var focusedWindow = _windowManager.GetFocusedWindow();
            if (focusedWindow != null)
            {
                _windowManager.ToggleFloating(focusedWindow.Handle);
                Console.WriteLine($"Toggled floating mode for {focusedWindow.Title}");
            }
        }

        private void ToggleFullscreen()
        {
            var currentLayout = _windowManager.GetCurrentLayoutMode();
            if (currentLayout == LayoutMode.Fullscreen)
            {
                _windowManager.SetLayoutMode(LayoutMode.MasterStack);
                Console.WriteLine("Exited fullscreen mode");
            }
            else
            {
                _windowManager.SetLayoutMode(LayoutMode.Fullscreen);
                Console.WriteLine("Entered fullscreen mode");
            }
        }

        private void CloseCurrentWindow()
        {
            var focusedWindow = _windowManager.GetFocusedWindow();
            if (focusedWindow != null)
            {
                _windowManager.CloseWindow(focusedWindow.Handle);
                Console.WriteLine($"Closed window: {focusedWindow.Title}");
            }
        }

        private void ExitManager()
        {
            Console.WriteLine("Exiting DWM Window Manager...");
            Environment.Exit(0);
        }

        private void MoveToNextMonitor()
        {
            // Implementation would move current window to next monitor
            Console.WriteLine("Move to next monitor");
        }

        private void MoveToPrevMonitor()
        {
            // Implementation would move current window to previous monitor
            Console.WriteLine("Move to previous monitor");
        }

        #endregion

        public void UnregisterHotKeys()
        {
            foreach (var hotKeyId in _hotKeyActions.Keys)
            {
                WinApi.UnregisterHotKey(_windowHandle, hotKeyId);
            }
            _hotKeyActions.Clear();
        }

        public void Dispose()
        {
            UnregisterHotKeys();
        }
    }
}