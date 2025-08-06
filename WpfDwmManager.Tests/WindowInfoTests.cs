using System;
using System.Drawing;
using Xunit;
using WpfDwmManager.Models;

namespace WpfDwmManager.Tests
{
    public class WindowInfoTests
    {
        [Fact]
        public void WindowInfo_Creation_ShouldSetProperties()
        {
            // Arrange
            var handle = new IntPtr(12345);
            var title = "Test Window";
            var bounds = new Rectangle(10, 20, 800, 600);

            // Act
            var windowInfo = new WindowInfo
            {
                Handle = handle,
                Title = title,
                Bounds = bounds,
                ProcessName = "TestProcess"
            };

            // Assert
            Assert.Equal(handle, windowInfo.Handle);
            Assert.Equal(title, windowInfo.Title);
            Assert.Equal(bounds, windowInfo.Bounds);
            Assert.Equal("TestProcess", windowInfo.ProcessName);
            Assert.False(windowInfo.IsFloating);
            Assert.False(windowInfo.IsMinimized);
            Assert.False(windowInfo.IsMaster);
        }

        [Fact]
        public void WindowInfo_Equals_ShouldCompareByHandle()
        {
            // Arrange
            var handle1 = new IntPtr(12345);
            var handle2 = new IntPtr(12345);
            var handle3 = new IntPtr(67890);

            var window1 = new WindowInfo { Handle = handle1, Title = "Window1" };
            var window2 = new WindowInfo { Handle = handle2, Title = "Window2" };
            var window3 = new WindowInfo { Handle = handle3, Title = "Window3" };

            // Act & Assert
            Assert.Equal(window1, window2); // Same handle, different titles
            Assert.NotEqual(window1, window3); // Different handles
        }

        [Fact]
        public void WindowInfo_GetHashCode_ShouldUseHandle()
        {
            // Arrange
            var handle = new IntPtr(12345);
            var window = new WindowInfo { Handle = handle };

            // Act
            var hashCode = window.GetHashCode();

            // Assert
            Assert.Equal(handle.GetHashCode(), hashCode);
        }

        [Fact]
        public void WindowInfo_ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var window = new WindowInfo
            {
                Handle = new IntPtr(12345),
                Title = "Test Window",
                ProcessName = "TestProcess"
            };

            // Act
            var result = window.ToString();

            // Assert
            Assert.Equal("Test Window (TestProcess) - 12345", result);
        }
    }
}