using System;
using System.Drawing;

namespace WpfDwmManager.Models
{
    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
        public Rectangle Bounds { get; set; }
        public bool IsFloating { get; set; }
        public bool IsMinimized { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public bool IsMaster { get; set; }
        public int ZOrder { get; set; }

        public override string ToString()
        {
            return $"{Title} ({ProcessName}) - {Handle}";
        }

        public override bool Equals(object? obj)
        {
            return obj is WindowInfo other && Handle == other.Handle;
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }
    }
}