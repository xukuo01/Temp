using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDwmManager.Services
{
    /// <summary>
    /// Simple task scheduler for running periodic window monitoring tasks
    /// </summary>
    public class WindowMonitoringService : IDisposable
    {
        private readonly WindowManager _windowManager;
        private readonly Timer _refreshTimer;
        private readonly Timer _focusTimer;
        private bool _disposed = false;

        public WindowMonitoringService(WindowManager windowManager)
        {
            _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
            
            // Refresh window list every 5 seconds
            _refreshTimer = new Timer(RefreshCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            
            // Check focus changes every 2 seconds
            _focusTimer = new Timer(FocusCallback, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }

        private void RefreshCallback(object? state)
        {
            try
            {
                var currentCount = _windowManager.GetManagedWindows().Count;
                _windowManager.RefreshWindowList();
                var newCount = _windowManager.GetManagedWindows().Count;
                
                if (currentCount != newCount)
                {
                    Console.WriteLine($"Window count changed: {currentCount} -> {newCount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during window refresh: {ex.Message}");
            }
        }

        private void FocusCallback(object? state)
        {
            try
            {
                // This could check for focus changes and update the focused window
                // For now, just ensure the layout is applied correctly
                var focusedWindow = _windowManager.GetFocusedWindow();
                if (focusedWindow != null)
                {
                    // Could trigger layout refresh if needed
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during focus check: {ex.Message}");
            }
        }

        public void Stop()
        {
            _refreshTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _focusTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Resume()
        {
            _refreshTimer?.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            _focusTimer?.Change(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _refreshTimer?.Dispose();
                _focusTimer?.Dispose();
                _disposed = true;
            }
        }
    }
}