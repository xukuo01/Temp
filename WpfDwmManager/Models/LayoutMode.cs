namespace WpfDwmManager.Models
{
    public enum LayoutMode
    {
        MasterStack,
        Floating,
        Fullscreen,
        Grid,
        Monocle
    }

    public class LayoutSettings
    {
        public double MasterRatio { get; set; } = 0.55;
        public int WindowGap { get; set; } = 5;
        public int BorderWidth { get; set; } = 2;
        public bool ShowBorders { get; set; } = true;
        public int GridColumns { get; set; } = 2;
        public int GridRows { get; set; } = 2;
    }
}