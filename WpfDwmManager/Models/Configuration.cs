using System.Collections.Generic;
using WpfDwmManager.Models;

namespace WpfDwmManager.Models
{
    public class Configuration
    {
        public LayoutSettings Layout { get; set; } = new LayoutSettings();
        public HotKeyConfig HotKeys { get; set; } = new HotKeyConfig();
        public List<string> WindowFilterRules { get; set; } = new List<string>();
        public bool EnableMultiMonitor { get; set; } = true;
        public bool AutoStartWithWindows { get; set; } = false;
        public string LogLevel { get; set; } = "Info";
    }

    public class HotKeyConfig
    {
        public string SwitchWindows { get; set; } = "Win+Tab";
        public string SetMaster { get; set; } = "Win+Shift+Return";
        public string FocusUp { get; set; } = "Win+K";
        public string FocusDown { get; set; } = "Win+J";
        public string ResizeLeft { get; set; } = "Win+H";
        public string ResizeRight { get; set; } = "Win+L";
        public string ToggleTiling { get; set; } = "Win+T";
        public string ToggleFloating { get; set; } = "Win+Space";
        public string ToggleFullscreen { get; set; } = "Win+F";
        public string CloseWindow { get; set; } = "Win+Q";
        public string ExitManager { get; set; } = "Win+Shift+C";
        public string MoveToNextMonitor { get; set; } = "Win+Shift+Right";
        public string MoveToPrevMonitor { get; set; } = "Win+Shift+Left";
    }
}