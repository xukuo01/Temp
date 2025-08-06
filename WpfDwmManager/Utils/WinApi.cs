using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace WpfDwmManager.Utils
{
    public static class WinApi
    {
        #region Constants
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_RESTORE = 9;

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_HIDEWINDOW = 0x0080;

        public const int GWL_STYLE = -16;
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_MAXIMIZE = 0x01000000;

        public const int WM_HOTKEY = 0x0312;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        public const uint EVENT_OBJECT_CREATE = 0x8000;
        public const uint EVENT_OBJECT_DESTROY = 0x8001;
        public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        #endregion

        #region Delegates
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        #endregion

        #region Cross-platform Windows API implementations
        public static bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return EnumWindowsNative(enumProc, lParam);
            }
            else
            {
                // Mock implementation for non-Windows platforms
                // Create some fake windows for testing
                var fakeWindows = new IntPtr[] { new IntPtr(1001), new IntPtr(1002), new IntPtr(1003) };
                foreach (var window in fakeWindows)
                {
                    if (!enumProc(window, lParam))
                        break;
                }
                return true;
            }
        }

        public static bool IsWindowVisible(IntPtr hWnd)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return IsWindowVisibleNative(hWnd);
            }
            return true; // Mock: assume visible
        }

        public static int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowTextNative(hWnd, lpString, nMaxCount);
            }
            
            // Mock implementation
            string mockTitle = $"Mock Window {hWnd.ToInt32()}";
            lpString.Clear();
            lpString.Append(mockTitle);
            return mockTitle.Length;
        }

        public static int GetWindowTextLength(IntPtr hWnd)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowTextLengthNative(hWnd);
            }
            return $"Mock Window {hWnd.ToInt32()}".Length;
        }

        public static bool GetWindowRect(IntPtr hWnd, out RECT lpRect)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowRectNative(hWnd, out lpRect);
            }
            
            // Mock implementation
            var random = new Random(hWnd.ToInt32());
            lpRect = new RECT
            {
                Left = random.Next(0, 800),
                Top = random.Next(0, 600),
                Right = random.Next(800, 1200),
                Bottom = random.Next(600, 900)
            };
            return true;
        }

        public static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return SetWindowPosNative(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);
            }
            
            Console.WriteLine($"Mock: SetWindowPos({hWnd}, {x}, {y}, {cx}, {cy})");
            return true;
        }

        public static bool ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ShowWindowNative(hWnd, nCmdShow);
            }
            
            Console.WriteLine($"Mock: ShowWindow({hWnd}, {nCmdShow})");
            return true;
        }

        public static bool SetForegroundWindow(IntPtr hWnd)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return SetForegroundWindowNative(hWnd);
            }
            
            Console.WriteLine($"Mock: SetForegroundWindow({hWnd})");
            return true;
        }

        public static IntPtr GetForegroundWindow()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetForegroundWindowNative();
            }
            return new IntPtr(1001); // Mock focused window
        }

        public static uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowThreadProcessIdNative(hWnd, out processId);
            }
            
            processId = (uint)hWnd.ToInt32();
            return processId;
        }

        public static int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowLongNative(hWnd, nIndex);
            }
            return 0;
        }

        public static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return SetWindowLongNative(hWnd, nIndex, dwNewLong);
            }
            return 0;
        }

        public static bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return RegisterHotKeyNative(hWnd, id, fsModifiers, vk);
            }
            
            Console.WriteLine($"Mock: RegisterHotKey(id={id}, modifiers={fsModifiers}, key={vk})");
            return true;
        }

        public static bool UnregisterHotKey(IntPtr hWnd, int id)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return UnregisterHotKeyNative(hWnd, id);
            }
            
            Console.WriteLine($"Mock: UnregisterHotKey(id={id})");
            return true;
        }

        public static IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return SetWinEventHookNative(eventMin, eventMax, hmodWinEventProc, lpfnWinEventProc, idProcess, idThread, dwFlags);
            }
            return new IntPtr(12345); // Mock hook handle
        }

        public static bool UnhookWinEvent(IntPtr hWinEventHook)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return UnhookWinEventNative(hWinEventHook);
            }
            return true;
        }

        public static bool CloseWindow(IntPtr hWnd)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CloseWindowNative(hWnd);
            }
            
            Console.WriteLine($"Mock: CloseWindow({hWnd})");
            return true;
        }

        public static bool IsIconic(IntPtr hWnd)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return IsIconicNative(hWnd);
            }
            return false;
        }

        public static bool IsZoomed(IntPtr hWnd)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return IsZoomedNative(hWnd);
            }
            return false;
        }
        #endregion

        #region Native Windows API calls (only available on Windows)
        [DllImport("user32.dll", EntryPoint = "EnumWindows")]
        private static extern bool EnumWindowsNative(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
        private static extern bool IsWindowVisibleNative(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText")]
        private static extern int GetWindowTextNative(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextLength")]
        private static extern int GetWindowTextLengthNative(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        private static extern bool GetWindowRectNative(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPosNative(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        private static extern bool ShowWindowNative(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern bool SetForegroundWindowNative(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        private static extern IntPtr GetForegroundWindowNative();

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        private static extern uint GetWindowThreadProcessIdNative(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLongNative(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLongNative(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "RegisterHotKey")]
        private static extern bool RegisterHotKeyNative(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", EntryPoint = "UnregisterHotKey")]
        private static extern bool UnregisterHotKeyNative(IntPtr hWnd, int id);

        [DllImport("user32.dll", EntryPoint = "SetWinEventHook")]
        private static extern IntPtr SetWinEventHookNative(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "UnhookWinEvent")]
        private static extern bool UnhookWinEventNative(IntPtr hWinEventHook);

        [DllImport("user32.dll", EntryPoint = "CloseWindow")]
        private static extern bool CloseWindowNative(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "IsIconic")]
        private static extern bool IsIconicNative(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "IsZoomed")]
        private static extern bool IsZoomedNative(IntPtr hWnd);
        #endregion

        #region Kernel32 APIs - Cross-platform wrappers
        public static IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OpenProcessNative(dwDesiredAccess, bInheritHandle, dwProcessId);
            }
            return new IntPtr((int)dwProcessId);
        }

        public static bool CloseHandle(IntPtr hObject)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CloseHandleNative(hObject);
            }
            return true;
        }

        public static bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return QueryFullProcessImageNameNative(hProcess, dwFlags, lpExeName, ref lpdwSize);
            }
            
            string mockProcessName = $"MockProcess{hProcess.ToInt32()}.exe";
            lpExeName.Clear();
            lpExeName.Append($"/mock/path/to/{mockProcessName}");
            lpdwSize = (uint)lpExeName.Length;
            return true;
        }

        [DllImport("kernel32.dll", EntryPoint = "OpenProcess")]
        private static extern IntPtr OpenProcessNative(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
        private static extern bool CloseHandleNative(IntPtr hObject);

        [DllImport("kernel32.dll", EntryPoint = "QueryFullProcessImageName")]
        private static extern bool QueryFullProcessImageNameNative(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize);
        #endregion

        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public Rectangle ToRectangle()
            {
                return new Rectangle(Left, Top, Right - Left, Bottom - Top);
            }
        }
        #endregion

        #region Helper Methods
        public static string GetWindowTitle(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;

            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string GetProcessName(IntPtr hWnd)
        {
            try
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                IntPtr processHandle = OpenProcess(0x1000, false, processId);
                if (processHandle == IntPtr.Zero) return string.Empty;

                uint bufferSize = 260;
                StringBuilder buffer = new StringBuilder((int)bufferSize);
                bool success = QueryFullProcessImageName(processHandle, 0, buffer, ref bufferSize);
                CloseHandle(processHandle);

                if (success)
                {
                    string fullPath = buffer.ToString();
                    return System.IO.Path.GetFileNameWithoutExtension(fullPath);
                }
            }
            catch
            {
                // Ignore errors
            }
            return string.Empty;
        }

        public static Rectangle GetWindowBounds(IntPtr hWnd)
        {
            if (GetWindowRect(hWnd, out RECT rect))
            {
                return rect.ToRectangle();
            }
            return Rectangle.Empty;
        }

        public static bool IsValidWindow(IntPtr hWnd)
        {
            if (!IsWindowVisible(hWnd)) return false;

            string title = GetWindowTitle(hWnd);
            if (string.IsNullOrEmpty(title)) return false;

            // Filter out system windows
            string processName = GetProcessName(hWnd);
            string[] systemProcesses = { "dwm", "winlogon", "csrss", "explorer", "taskmgr" };
            
            foreach (string sysProc in systemProcesses)
            {
                if (processName.Equals(sysProc, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }
        #endregion
    }
}