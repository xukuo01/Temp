using System;
using System.IO;
using Xunit;
using WpfDwmManager.Models;
using WpfDwmManager.Utils;

namespace WpfDwmManager.Tests
{
    public class ConfigManagerTests
    {
        [Fact]
        public void LoadConfiguration_DefaultConfig_ShouldReturnValidConfiguration()
        {
            // Act
            var config = ConfigManager.LoadConfiguration();

            // Assert
            Assert.NotNull(config);
            Assert.NotNull(config.Layout);
            Assert.NotNull(config.HotKeys);
            Assert.NotNull(config.WindowFilterRules);
            
            // Check default values
            Assert.Equal(0.55, config.Layout.MasterRatio);
            Assert.Equal(5, config.Layout.WindowGap);
            Assert.True(config.EnableMultiMonitor);
            Assert.Contains("dwm.exe", config.WindowFilterRules);
            Assert.Contains("explorer.exe", config.WindowFilterRules);
        }

        [Fact]
        public void LoadConfiguration_ShouldHaveAllHotKeys()
        {
            // Act
            var config = ConfigManager.LoadConfiguration();

            // Assert
            Assert.Equal("Win+Tab", config.HotKeys.SwitchWindows);
            Assert.Equal("Win+Shift+Return", config.HotKeys.SetMaster);
            Assert.Equal("Win+K", config.HotKeys.FocusUp);
            Assert.Equal("Win+J", config.HotKeys.FocusDown);
            Assert.Equal("Win+H", config.HotKeys.ResizeLeft);
            Assert.Equal("Win+L", config.HotKeys.ResizeRight);
            Assert.Equal("Win+T", config.HotKeys.ToggleTiling);
            Assert.Equal("Win+Space", config.HotKeys.ToggleFloating);
            Assert.Equal("Win+F", config.HotKeys.ToggleFullscreen);
            Assert.Equal("Win+Q", config.HotKeys.CloseWindow);
            Assert.Equal("Win+Shift+C", config.HotKeys.ExitManager);
        }

        [Fact]
        public void Configuration_DefaultLayoutSettings_ShouldBeValid()
        {
            // Arrange
            var layout = new LayoutSettings();

            // Assert
            Assert.Equal(0.55, layout.MasterRatio);
            Assert.Equal(5, layout.WindowGap);
            Assert.Equal(2, layout.BorderWidth);
            Assert.True(layout.ShowBorders);
            Assert.Equal(2, layout.GridColumns);
            Assert.Equal(2, layout.GridRows);
        }
    }
}