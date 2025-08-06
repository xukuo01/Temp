using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WpfDwmManager.Models;
using WpfDwmManager.Utils;

namespace WpfDwmManager.Services
{
    public class LayoutEngine
    {
        private readonly LayoutSettings _settings;

        public LayoutEngine()
        {
            var config = ConfigManager.LoadConfiguration();
            _settings = config.Layout;
        }

        public void ApplyMasterStackLayout(List<WindowInfo> windows, Rectangle screenBounds)
        {
            if (!windows.Any()) return;

            // Adjust for gaps
            var workArea = new Rectangle(
                screenBounds.X + _settings.WindowGap,
                screenBounds.Y + _settings.WindowGap,
                screenBounds.Width - (_settings.WindowGap * 2),
                screenBounds.Height - (_settings.WindowGap * 2));

            if (windows.Count == 1)
            {
                // Single window takes full screen
                SetWindowBounds(windows[0], workArea);
                return;
            }

            // Find master window (first one marked as master, or first window)
            var masterWindow = windows.FirstOrDefault(w => w.IsMaster) ?? windows.First();
            var stackWindows = windows.Where(w => w != masterWindow).ToList();

            // Calculate master area
            int masterWidth = (int)(workArea.Width * _settings.MasterRatio);
            var masterBounds = new Rectangle(
                workArea.X,
                workArea.Y,
                masterWidth - _settings.WindowGap / 2,
                workArea.Height);

            SetWindowBounds(masterWindow, masterBounds);

            // Calculate stack area
            if (stackWindows.Any())
            {
                int stackX = workArea.X + masterWidth + _settings.WindowGap / 2;
                int stackWidth = workArea.Width - masterWidth - _settings.WindowGap / 2;
                int stackHeight = workArea.Height / stackWindows.Count;

                for (int i = 0; i < stackWindows.Count; i++)
                {
                    var stackBounds = new Rectangle(
                        stackX,
                        workArea.Y + (i * stackHeight) + (i > 0 ? _settings.WindowGap : 0),
                        stackWidth,
                        stackHeight - (i < stackWindows.Count - 1 ? _settings.WindowGap : 0));

                    SetWindowBounds(stackWindows[i], stackBounds);
                }
            }
        }

        public void ApplyFloatingLayout(List<WindowInfo> windows)
        {
            // In floating mode, don't modify window positions
            // Windows maintain their current positions and can be moved freely
            foreach (var window in windows)
            {
                window.IsFloating = true;
            }
        }

        public void ApplyFullScreenLayout(WindowInfo activeWindow, Rectangle screenBounds)
        {
            if (activeWindow == null) return;

            SetWindowBounds(activeWindow, screenBounds);
            
            // Hide other windows or minimize them
            // For now, we'll just resize the active window to fullscreen
        }

        public void ApplyGridLayout(List<WindowInfo> windows, Rectangle screenBounds)
        {
            if (!windows.Any()) return;

            // Calculate grid dimensions
            int windowCount = windows.Count;
            int cols = Math.Min(_settings.GridColumns, windowCount);
            int rows = (int)Math.Ceiling((double)windowCount / cols);

            // Adjust for gaps
            var workArea = new Rectangle(
                screenBounds.X + _settings.WindowGap,
                screenBounds.Y + _settings.WindowGap,
                screenBounds.Width - (_settings.WindowGap * 2),
                screenBounds.Height - (_settings.WindowGap * 2));

            int cellWidth = workArea.Width / cols;
            int cellHeight = workArea.Height / rows;

            for (int i = 0; i < windows.Count; i++)
            {
                int col = i % cols;
                int row = i / cols;

                var bounds = new Rectangle(
                    workArea.X + (col * cellWidth) + (col > 0 ? _settings.WindowGap : 0),
                    workArea.Y + (row * cellHeight) + (row > 0 ? _settings.WindowGap : 0),
                    cellWidth - (col < cols - 1 ? _settings.WindowGap : 0),
                    cellHeight - (row < rows - 1 ? _settings.WindowGap : 0));

                SetWindowBounds(windows[i], bounds);
            }
        }

        public void ApplyMonocleLayout(List<WindowInfo> windows, Rectangle screenBounds, WindowInfo activeWindow)
        {
            if (activeWindow == null) return;

            // In monocle mode, only the active window is visible at full screen
            SetWindowBounds(activeWindow, screenBounds);
            
            // Other windows are minimized or hidden
            foreach (var window in windows.Where(w => w != activeWindow))
            {
                WinApi.ShowWindow(window.Handle, WinApi.SW_SHOWMINIMIZED);
            }
        }

        private void SetWindowBounds(WindowInfo window, Rectangle bounds)
        {
            // Store the new bounds
            window.Bounds = bounds;
            
            // Apply the bounds to the actual window
            WinApi.SetWindowPos(
                window.Handle,
                IntPtr.Zero,
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                WinApi.SWP_NOZORDER | WinApi.SWP_NOACTIVATE);
        }

        public void ResizeMasterArea(double delta)
        {
            _settings.MasterRatio = Math.Max(0.1, Math.Min(0.9, _settings.MasterRatio + delta));
            
            // Save updated settings
            var config = ConfigManager.LoadConfiguration();
            config.Layout = _settings;
            ConfigManager.SaveConfiguration(config);
        }

        public void SetWindowGap(int gap)
        {
            _settings.WindowGap = Math.Max(0, gap);
            
            // Save updated settings
            var config = ConfigManager.LoadConfiguration();
            config.Layout = _settings;
            ConfigManager.SaveConfiguration(config);
        }
    }
}