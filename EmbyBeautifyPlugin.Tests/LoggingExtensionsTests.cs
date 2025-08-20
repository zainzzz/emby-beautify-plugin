using EmbyBeautifyPlugin.Exceptions;
using EmbyBeautifyPlugin.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// Unit tests for LoggingExtensions
    /// </summary>
    public class LoggingExtensionsTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public LoggingExtensionsTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void LogBeautifyException_WithNullLogger_ShouldNotThrow()
        {
            // Arrange
            ILogger logger = null;
            var exception = new BeautifyException();

            // Act & Assert
            logger.LogBeautifyException(exception); // Should not throw
        }

        [Fact]
        public void LogBeautifyException_WithNullException_ShouldNotThrow()
        {
            // Act & Assert
            _mockLogger.Object.LogBeautifyException(null); // Should not throw
        }

        [Fact]
        public void LogBeautifyException_WithValidException_ShouldLogWithCorrectLevel()
        {
            // Arrange
            var exception = new BeautifyException(BeautifyErrorType.ConfigurationError, "Config error");

            // Act
            _mockLogger.Object.LogBeautifyException(exception);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogBeautifyException_WithCustomMessage_ShouldIncludeCustomMessage()
        {
            // Arrange
            var exception = new BeautifyException(BeautifyErrorType.ThemeLoadError, "Theme error");
            var customMessage = "Custom error message";

            // Act
            _mockLogger.Object.LogBeautifyException(exception, customMessage);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(customMessage)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogOperationStart_WithNullLogger_ShouldNotThrow()
        {
            // Arrange
            ILogger logger = null;

            // Act & Assert
            logger.LogOperationStart("test operation"); // Should not throw
        }

        [Fact]
        public void LogOperationStart_WithEmptyOperation_ShouldNotLog()
        {
            // Act
            _mockLogger.Object.LogOperationStart("");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogOperationStart_WithValidOperation_ShouldLogDebug()
        {
            // Arrange
            var operation = "TestOperation";

            // Act
            _mockLogger.Object.LogOperationStart(operation);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Starting operation: {operation}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogOperationStart_WithContext_ShouldIncludeContext()
        {
            // Arrange
            var operation = "TestOperation";
            var context = new Dictionary<string, object>
            {
                { "Key1", "Value1" },
                { "Key2", 42 }
            };

            // Act
            _mockLogger.Object.LogOperationStart(operation, context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Key1=Value1") && v.ToString().Contains("Key2=42")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogOperationComplete_WithSuccessfulOperation_ShouldLogDebug()
        {
            // Arrange
            var operation = "TestOperation";
            var duration = TimeSpan.FromMilliseconds(150);

            // Act
            _mockLogger.Object.LogOperationComplete(operation, duration, true);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("completed successfully") && v.ToString().Contains("150.00ms")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogOperationComplete_WithFailedOperation_ShouldLogWarning()
        {
            // Arrange
            var operation = "TestOperation";
            var duration = TimeSpan.FromMilliseconds(100);

            // Act
            _mockLogger.Object.LogOperationComplete(operation, duration, false);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogConfigurationChange_WithChanges_ShouldLogInformation()
        {
            // Arrange
            var configType = "ThemeConfig";
            var changes = new Dictionary<string, object>
            {
                { "ActiveTheme", "DarkTheme" },
                { "EnableAnimations", true }
            };

            // Act
            _mockLogger.Object.LogConfigurationChange(configType, changes);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Configuration changed: ThemeConfig") &&
                                                  v.ToString().Contains("ActiveTheme: DarkTheme")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogThemeOperation_WithSuccessfulOperation_ShouldLogInformation()
        {
            // Arrange
            var operation = "LoadTheme";
            var themeName = "DarkTheme";
            var details = "Theme loaded successfully";

            // Act
            _mockLogger.Object.LogThemeOperation(operation, themeName, true, details);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Theme operation 'LoadTheme' successful") &&
                                                  v.ToString().Contains("for theme 'DarkTheme'") &&
                                                  v.ToString().Contains("Theme loaded successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogThemeOperation_WithFailedOperation_ShouldLogWarning()
        {
            // Arrange
            var operation = "LoadTheme";
            var themeName = "InvalidTheme";

            // Act
            _mockLogger.Object.LogThemeOperation(operation, themeName, false);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Theme operation 'LoadTheme' failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogPerformanceMetric_WithMetric_ShouldLogDebug()
        {
            // Arrange
            var metric = "ThemeLoadTime";
            var value = 125.5;
            var unit = "ms";
            var context = new Dictionary<string, object> { { "ThemeName", "DarkTheme" } };

            // Act
            _mockLogger.Object.LogPerformanceMetric(metric, value, unit, context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Performance metric: ThemeLoadTime = 125.50ms") &&
                                                  v.ToString().Contains("ThemeName=DarkTheme")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogSecurityEvent_WithEvent_ShouldLogAtSpecifiedLevel()
        {
            // Arrange
            var eventType = "UnauthorizedAccess";
            var details = "Attempted access to restricted theme";
            var severity = LogLevel.Error;

            // Act
            _mockLogger.Object.LogSecurityEvent(eventType, details, severity);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Security event: UnauthorizedAccess") &&
                                                  v.ToString().Contains("Attempted access to restricted theme")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(BeautifyErrorType.InitializationError, LogLevel.Critical)]
        [InlineData(BeautifyErrorType.ConfigurationError, LogLevel.Error)]
        [InlineData(BeautifyErrorType.FileSystemError, LogLevel.Error)]
        [InlineData(BeautifyErrorType.ThemeLoadError, LogLevel.Warning)]
        [InlineData(BeautifyErrorType.StyleInjectionError, LogLevel.Warning)]
        [InlineData(BeautifyErrorType.ValidationError, LogLevel.Warning)]
        [InlineData(BeautifyErrorType.NetworkError, LogLevel.Warning)]
        [InlineData(BeautifyErrorType.CompatibilityError, LogLevel.Information)]
        [InlineData(BeautifyErrorType.UnknownError, LogLevel.Error)]
        public void LogBeautifyException_WithDifferentErrorTypes_ShouldLogAtCorrectLevel(
            BeautifyErrorType errorType, LogLevel expectedLogLevel)
        {
            // Arrange
            var exception = new BeautifyException(errorType, "Test error");

            // Act
            _mockLogger.Object.LogBeautifyException(exception);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    expectedLogLevel,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// Unit tests for OperationTimer
    /// </summary>
    public class OperationTimerTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public OperationTimerTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OperationTimer(null, "test"));
        }

        [Fact]
        public void Constructor_WithNullOperation_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OperationTimer(_mockLogger.Object, null));
        }

        [Fact]
        public void Constructor_ShouldLogOperationStart()
        {
            // Act
            using (var timer = new OperationTimer(_mockLogger.Object, "TestOperation"))
            {
                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Debug,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting operation: TestOperation")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        [Fact]
        public void Dispose_ShouldLogOperationComplete()
        {
            // Act
            using (var timer = new OperationTimer(_mockLogger.Object, "TestOperation"))
            {
                // Timer will be disposed here
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Operation TestOperation completed successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Complete_WithSuccess_ShouldLogSuccessfulCompletion()
        {
            // Arrange
            var timer = new OperationTimer(_mockLogger.Object, "TestOperation");

            // Act
            timer.Complete(true);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("completed successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Complete_WithFailure_ShouldLogFailedCompletion()
        {
            // Arrange
            var timer = new OperationTimer(_mockLogger.Object, "TestOperation");

            // Act
            timer.Complete(false);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void BeginOperation_ShouldReturnOperationTimer()
        {
            // Act
            var timer = _mockLogger.Object.BeginOperation("TestOperation");

            // Assert
            Assert.NotNull(timer);
            Assert.IsType<OperationTimer>(timer);
            
            timer.Dispose();
        }

        [Fact]
        public void BeginOperation_WithContext_ShouldPassContextToTimer()
        {
            // Arrange
            var context = new Dictionary<string, object> { { "Key", "Value" } };

            // Act
            using (var timer = _mockLogger.Object.BeginOperation("TestOperation", context))
            {
                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Debug,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Key=Value")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}