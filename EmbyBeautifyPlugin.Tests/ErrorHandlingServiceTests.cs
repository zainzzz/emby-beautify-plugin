using EmbyBeautifyPlugin.Exceptions;
using EmbyBeautifyPlugin.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// Unit tests for ErrorHandlingService
    /// </summary>
    public class ErrorHandlingServiceTests
    {
        private readonly Mock<ILogger<ErrorHandlingService>> _mockLogger;
        private readonly ErrorHandlingService _errorHandlingService;

        public ErrorHandlingServiceTests()
        {
            _mockLogger = new Mock<ILogger<ErrorHandlingService>>();
            _errorHandlingService = new ErrorHandlingService(_mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ErrorHandlingService(null));
        }

        [Fact]
        public async Task HandleExceptionAsync_WithNullException_ShouldLogWarning()
        {
            // Act
            await _errorHandlingService.HandleExceptionAsync((BeautifyException)null);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempted to handle null exception")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleExceptionAsync_WithBeautifyException_ShouldLogAppropriately()
        {
            // Arrange
            var exception = new BeautifyException(BeautifyErrorType.ConfigurationError, "Test error")
                .SetComponent("TestComponent");

            // Act
            await _errorHandlingService.HandleExceptionAsync(exception);

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
        public async Task HandleExceptionAsync_WithContext_ShouldAddContextToException()
        {
            // Arrange
            var exception = new BeautifyException(BeautifyErrorType.ThemeLoadError, "Theme error");
            var context = new Dictionary<string, object>
            {
                { "ThemeName", "DarkTheme" },
                { "Version", "1.0" }
            };

            // Act
            await _errorHandlingService.HandleExceptionAsync(exception, context);

            // Assert
            Assert.True(exception.ErrorContext.ContainsKey("ThemeName"));
            Assert.True(exception.ErrorContext.ContainsKey("Version"));
            Assert.Equal("DarkTheme", exception.ErrorContext["ThemeName"]);
            Assert.Equal("1.0", exception.ErrorContext["Version"]);
        }

        [Fact]
        public async Task HandleExceptionAsync_WithGeneralException_ShouldWrapInBeautifyException()
        {
            // Arrange
            var originalException = new InvalidOperationException("Original error");
            var errorType = BeautifyErrorType.ValidationError;
            var component = "TestComponent";
            var userMessage = "User friendly message";

            // Act
            await _errorHandlingService.HandleExceptionAsync(originalException, errorType, component, userMessage);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<BeautifyException>(ex => 
                        ex.ErrorType == errorType && 
                        ex.Component == component && 
                        ex.UserMessage == userMessage &&
                        ex.InnerException == originalException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteSafelyAsync_WithSuccessfulAction_ShouldExecuteWithoutException()
        {
            // Arrange
            var executed = false;
            Func<Task> action = () =>
            {
                executed = true;
                return Task.CompletedTask;
            };

            // Act
            await _errorHandlingService.ExecuteSafelyAsync(action, BeautifyErrorType.UnknownError);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public async Task ExecuteSafelyAsync_WithFailingAction_ShouldHandleExceptionAndRethrow()
        {
            // Arrange
            var originalException = new InvalidOperationException("Test error");
            Func<Task> action = () => throw originalException;

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _errorHandlingService.ExecuteSafelyAsync(action, BeautifyErrorType.ConfigurationError, "TestComponent"));

            Assert.Same(originalException, thrownException);
        }

        [Fact]
        public async Task ExecuteSafelyAsync_WithBeautifyException_ShouldRethrowWithoutHandling()
        {
            // Arrange
            var beautifyException = new BeautifyException(BeautifyErrorType.ThemeLoadError, "Theme error");
            Func<Task> action = () => throw beautifyException;

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<BeautifyException>(
                () => _errorHandlingService.ExecuteSafelyAsync(action, BeautifyErrorType.ConfigurationError));

            Assert.Same(beautifyException, thrownException);
        }

        [Fact]
        public async Task ExecuteSafelyAsync_WithResult_WithSuccessfulFunction_ShouldReturnResult()
        {
            // Arrange
            var expectedResult = "test result";
            Func<Task<string>> func = () => Task.FromResult(expectedResult);

            // Act
            var result = await _errorHandlingService.ExecuteSafelyAsync(func, "default", BeautifyErrorType.UnknownError);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ExecuteSafelyAsync_WithResult_WithFailingFunction_ShouldReturnDefaultValue()
        {
            // Arrange
            var defaultValue = "default value";
            Func<Task<string>> func = () => throw new InvalidOperationException("Test error");

            // Act
            var result = await _errorHandlingService.ExecuteSafelyAsync(func, defaultValue, BeautifyErrorType.NetworkError);

            // Assert
            Assert.Equal(defaultValue, result);
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
        public async Task HandleExceptionAsync_WithDifferentErrorTypes_ShouldLogAtCorrectLevel(
            BeautifyErrorType errorType, LogLevel expectedLogLevel)
        {
            // Arrange
            var exception = new BeautifyException(errorType, "Test error");

            // Act
            await _errorHandlingService.HandleExceptionAsync(exception);

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

        [Fact]
        public async Task HandleExceptionAsync_WithCriticalErrorInHandling_ShouldNotThrow()
        {
            // Arrange
            var exception = new BeautifyException(BeautifyErrorType.ConfigurationError, "Test error");
            
            // Setup logger to throw exception during logging
            _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Throws(new InvalidOperationException("Logger error"));

            // Act & Assert - Should not throw even when logger fails
            await _errorHandlingService.HandleExceptionAsync(exception);
            
            // The test passes if no exception is thrown
        }

        [Fact]
        public async Task HandleExceptionAsync_WithUnknownErrorType_ShouldUseUnknownErrorStrategy()
        {
            // Arrange
            var exception = new BeautifyException((BeautifyErrorType)999, "Unknown error type");

            // Act
            await _errorHandlingService.HandleExceptionAsync(exception);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleExceptionAsync_WithContextAndGeneralException_ShouldAddContextToWrappedException()
        {
            // Arrange
            var originalException = new ArgumentException("Invalid argument");
            var context = new Dictionary<string, object>
            {
                { "Parameter", "testParam" },
                { "Value", 42 }
            };

            // Act
            await _errorHandlingService.HandleExceptionAsync(originalException, BeautifyErrorType.ValidationError, 
                "TestComponent", "Validation failed", context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<BeautifyException>(ex => 
                        ex.ErrorContext.ContainsKey("Parameter") &&
                        ex.ErrorContext.ContainsKey("Value") &&
                        ex.ErrorContext["Parameter"].Equals("testParam") &&
                        ex.ErrorContext["Value"].Equals(42)),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}