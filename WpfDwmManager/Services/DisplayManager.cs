using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WpfDwmManager.Services
{
    public class DisplayManager
    {
        private List<Rectangle> _displays = new List<Rectangle>();

        public DisplayManager()
        {
            RefreshDisplays();
        }

        public void RefreshDisplays()
        {
            _displays.Clear();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, would use Screen.AllScreens from System.Windows.Forms
                // For now, add a default display
                _displays.Add(new Rectangle(0, 0, 1920, 1080));
            }
            else
            {
                // Mock displays for non-Windows platforms
                _displays.Add(new Rectangle(0, 0, 1920, 1080));
                _displays.Add(new Rectangle(1920, 0, 1920, 1080)); // Second monitor
            }
        }

        public Rectangle GetPrimaryDisplay()
        {
            return _displays.Count > 0 ? _displays[0] : new Rectangle(0, 0, 1920, 1080);
        }

        public List<Rectangle> GetAllDisplays()
        {
            return new List<Rectangle>(_displays);
        }

        public Rectangle GetDisplayContaining(Rectangle windowBounds)
        {
            foreach (var display in _displays)
            {
                if (display.IntersectsWith(windowBounds))
                {
                    return display;
                }
            }
            return GetPrimaryDisplay();
        }

        public Rectangle GetNextDisplay(Rectangle currentDisplay)
        {
            if (_displays.Count <= 1) return currentDisplay;

            int currentIndex = _displays.FindIndex(d => d == currentDisplay);
            if (currentIndex == -1) return GetPrimaryDisplay();

            int nextIndex = (currentIndex + 1) % _displays.Count;
            return _displays[nextIndex];
        }

        public Rectangle GetPreviousDisplay(Rectangle currentDisplay)
        {
            if (_displays.Count <= 1) return currentDisplay;

            int currentIndex = _displays.FindIndex(d => d == currentDisplay);
            if (currentIndex == -1) return GetPrimaryDisplay();

            int prevIndex = currentIndex == 0 ? _displays.Count - 1 : currentIndex - 1;
            return _displays[prevIndex];
        }

        public int GetDisplayCount()
        {
            return _displays.Count;
        }

        public bool IsMultiMonitorSetup()
        {
            return _displays.Count > 1;
        }
    }
}