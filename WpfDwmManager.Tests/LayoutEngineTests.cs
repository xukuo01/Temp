using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xunit;
using WpfDwmManager.Models;
using WpfDwmManager.Services;

namespace WpfDwmManager.Tests
{
    public class LayoutEngineTests
    {
        [Fact]
        public void ApplyMasterStackLayout_SingleWindow_ShouldFillScreen()
        {
            // Arrange
            var layoutEngine = new LayoutEngine();
            var screenBounds = new Rectangle(0, 0, 1920, 1080);
            var windows = new List<WindowInfo>
            {
                new WindowInfo { Handle = new IntPtr(1), Title = "Window1" }
            };

            // Act
            layoutEngine.ApplyMasterStackLayout(windows, screenBounds);

            // Assert
            var window = windows.First();
            Assert.True(window.Bounds.Width > 1900); // Should be nearly full width (minus gaps)
            Assert.True(window.Bounds.Height > 1050); // Should be nearly full height (minus gaps)
        }

        [Fact]
        public void ApplyMasterStackLayout_MultipleWindows_ShouldCreateMasterAndStack()
        {
            // Arrange
            var layoutEngine = new LayoutEngine();
            var screenBounds = new Rectangle(0, 0, 1920, 1080);
            var windows = new List<WindowInfo>
            {
                new WindowInfo { Handle = new IntPtr(1), Title = "Window1", IsMaster = true },
                new WindowInfo { Handle = new IntPtr(2), Title = "Window2" },
                new WindowInfo { Handle = new IntPtr(3), Title = "Window3" }
            };

            // Act
            layoutEngine.ApplyMasterStackLayout(windows, screenBounds);

            // Assert
            var masterWindow = windows.First(w => w.IsMaster);
            var stackWindows = windows.Where(w => !w.IsMaster).ToList();

            // Master should take ~55% of width (default ratio)
            Assert.True(masterWindow.Bounds.Width > 1000 && masterWindow.Bounds.Width < 1100);
            
            // Stack windows should be smaller and positioned to the right
            foreach (var stackWindow in stackWindows)
            {
                Assert.True(stackWindow.Bounds.X > masterWindow.Bounds.Width);
                Assert.True(stackWindow.Bounds.Width < masterWindow.Bounds.Width);
            }
        }

        [Fact]
        public void ApplyGridLayout_FourWindows_ShouldCreateTwoByTwoGrid()
        {
            // Arrange
            var layoutEngine = new LayoutEngine();
            var screenBounds = new Rectangle(0, 0, 1920, 1080);
            var windows = new List<WindowInfo>
            {
                new WindowInfo { Handle = new IntPtr(1), Title = "Window1" },
                new WindowInfo { Handle = new IntPtr(2), Title = "Window2" },
                new WindowInfo { Handle = new IntPtr(3), Title = "Window3" },
                new WindowInfo { Handle = new IntPtr(4), Title = "Window4" }
            };

            // Act
            layoutEngine.ApplyGridLayout(windows, screenBounds);

            // Assert
            foreach (var window in windows)
            {
                // Each window should take roughly 1/4 of the screen (accounting for gaps)
                Assert.True(window.Bounds.Width > 900 && window.Bounds.Width < 1000);
                Assert.True(window.Bounds.Height > 500 && window.Bounds.Height < 600);
            }

            // Check positioning - windows should be in grid formation
            var topLeftWindow = windows.OrderBy(w => w.Bounds.Y).ThenBy(w => w.Bounds.X).First();
            var topRightWindow = windows.Where(w => w.Bounds.Y == topLeftWindow.Bounds.Y && w.Bounds.X > topLeftWindow.Bounds.X).FirstOrDefault();
            
            Assert.NotNull(topRightWindow);
            Assert.True(topRightWindow.Bounds.X > topLeftWindow.Bounds.Right);
        }

        [Fact]
        public void ApplyFloatingLayout_ShouldSetFloatingFlag()
        {
            // Arrange
            var layoutEngine = new LayoutEngine();
            var windows = new List<WindowInfo>
            {
                new WindowInfo { Handle = new IntPtr(1), Title = "Window1", IsFloating = false },
                new WindowInfo { Handle = new IntPtr(2), Title = "Window2", IsFloating = false }
            };

            // Act
            layoutEngine.ApplyFloatingLayout(windows);

            // Assert
            Assert.All(windows, window => Assert.True(window.IsFloating));
        }

        [Fact]
        public void ApplyFullScreenLayout_ShouldMatchScreenBounds()
        {
            // Arrange
            var layoutEngine = new LayoutEngine();
            var screenBounds = new Rectangle(0, 0, 1920, 1080);
            var window = new WindowInfo { Handle = new IntPtr(1), Title = "Window1" };

            // Act
            layoutEngine.ApplyFullScreenLayout(window, screenBounds);

            // Assert
            Assert.Equal(screenBounds, window.Bounds);
        }
    }
}