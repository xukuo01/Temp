using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using WpfDwmManager.Models;
using WpfDwmManager.Utils;

namespace WpfDwmManager.Services
{
    public class WindowManager : IDisposable
    {
        private readonly List<WindowInfo> _managedWindows = new List<WindowInfo>();
        private readonly LayoutEngine _layoutEngine;
        private readonly DisplayManager _displayManager;
        private IntPtr _winEventHook;
        private WinApi.WinEventDelegate? _winEventDelegate;
        private WindowInfo? _focusedWindow;
        private LayoutMode _currentLayout = LayoutMode.MasterStack;

        public event EventHandler<WindowInfo>? WindowCreated;
        public event EventHandler<WindowInfo>? WindowDestroyed;
        public event EventHandler<WindowInfo>? WindowFocused;

        public WindowManager()
        {
            _layoutEngine = new LayoutEngine();
            _displayManager = new DisplayManager();
        }

        public void Initialize()
        {
            RefreshWindowList();
            SetupWindowEventHooks();
            ApplyCurrentLayout();
        }

        public void RefreshWindowList()
        {
            _managedWindows.Clear();
            WinApi.EnumWindows(EnumWindowsCallback, IntPtr.Zero);
            
            Debug.WriteLine($"Found {_managedWindows.Count} managed windows");
            foreach (var window in _managedWindows)
            {
                Debug.WriteLine($"  {window}");
            }
        }

        private bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            if (WinApi.IsValidWindow(hWnd))
            {
                var windowInfo = CreateWindowInfo(hWnd);
                if (windowInfo != null && !IsFiltered(windowInfo))
                {
                    _managedWindows.Add(windowInfo);
                }
            }
            return true;
        }

        private WindowInfo? CreateWindowInfo(IntPtr hWnd)
        {
            try
            {
                return new WindowInfo
                {
                    Handle = hWnd,
                    Title = WinApi.GetWindowTitle(hWnd),
                    ProcessName = WinApi.GetProcessName(hWnd),
                    Bounds = WinApi.GetWindowBounds(hWnd),
                    IsMinimized = WinApi.IsIconic(hWnd),
                    IsFloating = false
                };
            }
            catch
            {
                return null;
            }
        }

        private bool IsFiltered(WindowInfo window)
        {
            var config = ConfigManager.LoadConfiguration();
            return config.WindowFilterRules.Any(rule => 
                window.ProcessName.Contains(rule, StringComparison.OrdinalIgnoreCase) ||
                window.Title.Contains(rule, StringComparison.OrdinalIgnoreCase));
        }

        private void SetupWindowEventHooks()
        {
            _winEventDelegate = WinEventProc;
            _winEventHook = WinApi.SetWinEventHook(
                WinApi.EVENT_SYSTEM_FOREGROUND,
                WinApi.EVENT_OBJECT_DESTROY,
                IntPtr.Zero,
                _winEventDelegate,
                0, 0,
                WinApi.WINEVENT_OUTOFCONTEXT);
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero) return;

            switch (eventType)
            {
                case WinApi.EVENT_SYSTEM_FOREGROUND:
                    HandleWindowFocused(hwnd);
                    break;
                case WinApi.EVENT_OBJECT_CREATE:
                    HandleWindowCreated(hwnd);
                    break;
                case WinApi.EVENT_OBJECT_DESTROY:
                    HandleWindowDestroyed(hwnd);
                    break;
            }
        }

        private void HandleWindowFocused(IntPtr hwnd)
        {
            var window = _managedWindows.FirstOrDefault(w => w.Handle == hwnd);
            if (window != null)
            {
                _focusedWindow = window;
                WindowFocused?.Invoke(this, window);
            }
        }

        private void HandleWindowCreated(IntPtr hwnd)
        {
            if (WinApi.IsValidWindow(hwnd))
            {
                var windowInfo = CreateWindowInfo(hwnd);
                if (windowInfo != null && !IsFiltered(windowInfo))
                {
                    _managedWindows.Add(windowInfo);
                    WindowCreated?.Invoke(this, windowInfo);
                    ApplyCurrentLayout();
                }
            }
        }

        private void HandleWindowDestroyed(IntPtr hwnd)
        {
            var window = _managedWindows.FirstOrDefault(w => w.Handle == hwnd);
            if (window != null)
            {
                _managedWindows.Remove(window);
                WindowDestroyed?.Invoke(this, window);
                ApplyCurrentLayout();
            }
        }

        public List<WindowInfo> GetManagedWindows()
        {
            return new List<WindowInfo>(_managedWindows);
        }

        public WindowInfo? GetFocusedWindow()
        {
            return _focusedWindow;
        }

        public void SetWindowPosition(IntPtr handle, Rectangle bounds)
        {
            WinApi.SetWindowPos(handle, IntPtr.Zero, bounds.X, bounds.Y, bounds.Width, bounds.Height, 
                WinApi.SWP_NOZORDER | WinApi.SWP_NOACTIVATE);
        }

        public void FocusWindow(IntPtr handle)
        {
            WinApi.SetForegroundWindow(handle);
        }

        public void CloseWindow(IntPtr handle)
        {
            WinApi.CloseWindow(handle);
        }

        public void SetLayoutMode(LayoutMode mode)
        {
            _currentLayout = mode;
            ApplyCurrentLayout();
        }

        public LayoutMode GetCurrentLayoutMode()
        {
            return _currentLayout;
        }

        private void ApplyCurrentLayout()
        {
            if (!_managedWindows.Any()) return;

            var visibleWindows = _managedWindows.Where(w => !w.IsMinimized && !w.IsFloating).ToList();
            if (!visibleWindows.Any()) return;

            var primaryScreen = _displayManager.GetPrimaryDisplay();
            
            switch (_currentLayout)
            {
                case LayoutMode.MasterStack:
                    _layoutEngine.ApplyMasterStackLayout(visibleWindows, primaryScreen);
                    break;
                case LayoutMode.Floating:
                    _layoutEngine.ApplyFloatingLayout(visibleWindows);
                    break;
                case LayoutMode.Fullscreen:
                    if (_focusedWindow != null)
                        _layoutEngine.ApplyFullScreenLayout(_focusedWindow, primaryScreen);
                    break;
                case LayoutMode.Grid:
                    _layoutEngine.ApplyGridLayout(visibleWindows, primaryScreen);
                    break;
            }
        }

        public void SwitchToNextWindow()
        {
            if (_managedWindows.Count <= 1) return;

            int currentIndex = _focusedWindow != null ? _managedWindows.IndexOf(_focusedWindow) : -1;
            int nextIndex = (currentIndex + 1) % _managedWindows.Count;
            
            FocusWindow(_managedWindows[nextIndex].Handle);
        }

        public void SetMasterWindow(IntPtr handle)
        {
            var window = _managedWindows.FirstOrDefault(w => w.Handle == handle);
            if (window != null)
            {
                // Clear other master flags
                foreach (var w in _managedWindows)
                    w.IsMaster = false;
                
                window.IsMaster = true;
                ApplyCurrentLayout();
            }
        }

        public void ToggleFloating(IntPtr handle)
        {
            var window = _managedWindows.FirstOrDefault(w => w.Handle == handle);
            if (window != null)
            {
                window.IsFloating = !window.IsFloating;
                ApplyCurrentLayout();
            }
        }

        public void Dispose()
        {
            if (_winEventHook != IntPtr.Zero)
            {
                WinApi.UnhookWinEvent(_winEventHook);
                _winEventHook = IntPtr.Zero;
            }
            _winEventDelegate = null;
        }
    }
}