using EmbyBeautifyPlugin.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// Unit tests for BeautifyException
    /// </summary>
    public class BeautifyExceptionTests
    {
        [Fact]
        public void Constructor_Default_ShouldInitializeWithDefaultValues()
        {
            // Act
            var exception = new BeautifyException();

            // Assert
            Assert.Equal(BeautifyErrorType.UnknownError, exception.ErrorType);
            Assert.Equal("An unexpected error occurred in the beautify plugin.", exception.UserMessage);
            Assert.NotNull(exception.ErrorContext);
            Assert.Empty(exception.ErrorContext);
            Assert.Equal("Unknown", exception.Component);
            Assert.True(exception.Timestamp <= DateTime.UtcNow);
            Assert.True(exception.Timestamp > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public void Constructor_WithMessage_ShouldSetMessage()
        {
            // Arrange
            var message = "Test error message";

            // Act
            var exception = new BeautifyException(message);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(BeautifyErrorType.UnknownError, exception.ErrorType);
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
        {
            // Arrange
            var message = "Test error message";
            var innerException = new InvalidOperationException("Inner exception");

            // Act
            var exception = new BeautifyException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

        [Fact]
        public void Constructor_WithErrorTypeAndMessage_ShouldSetBoth()
        {
            // Arrange
            var errorType = BeautifyErrorType.ConfigurationError;
            var message = "Configuration error occurred";

            // Act
            var exception = new BeautifyException(errorType, message);

            // Assert
            Assert.Equal(errorType, exception.ErrorType);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void Constructor_WithAllParameters_ShouldSetAllProperties()
        {
            // Arrange
            var errorType = BeautifyErrorType.ThemeLoadError;
            var message = "Theme loading failed";
            var userMessage = "Failed to load theme";
            var component = "ThemeManager";

            // Act
            var exception = new BeautifyException(errorType, message, userMessage, component);

            // Assert
            Assert.Equal(errorType, exception.ErrorType);
            Assert.Equal(message, exception.Message);
            Assert.Equal(userMessage, exception.UserMessage);
            Assert.Equal(component, exception.Component);
        }

        [Fact]
        public void AddContext_WithValidKeyValue_ShouldAddToContext()
        {
            // Arrange
            var exception = new BeautifyException();
            var key = "TestKey";
            var value = "TestValue";

            // Act
            var result = exception.AddContext(key, value);

            // Assert
            Assert.Same(exception, result); // Should return same instance for chaining
            Assert.True(exception.ErrorContext.ContainsKey(key));
            Assert.Equal(value, exception.ErrorContext[key]);
        }

        [Fact]
        public void AddContext_WithNullKey_ShouldNotAddToContext()
        {
            // Arrange
            var exception = new BeautifyException();

            // Act
            exception.AddContext(null, "value");

            // Assert
            Assert.Empty(exception.ErrorContext);
        }

        [Fact]
        public void AddContext_WithNullValue_ShouldNotAddToContext()
        {
            // Arrange
            var exception = new BeautifyException();

            // Act
            exception.AddContext("key", null);

            // Assert
            Assert.Empty(exception.ErrorContext);
        }

        [Fact]
        public void SetComponent_WithValidComponent_ShouldSetComponent()
        {
            // Arrange
            var exception = new BeautifyException();
            var component = "TestComponent";

            // Act
            var result = exception.SetComponent(component);

            // Assert
            Assert.Same(exception, result); // Should return same instance for chaining
            Assert.Equal(component, exception.Component);
        }

        [Fact]
        public void SetComponent_WithNullComponent_ShouldSetToUnknown()
        {
            // Arrange
            var exception = new BeautifyException();

            // Act
            exception.SetComponent(null);

            // Assert
            Assert.Equal("Unknown", exception.Component);
        }

        [Fact]
        public void GetFormattedMessage_WithoutContext_ShouldReturnBasicFormat()
        {
            // Arrange
            var exception = new BeautifyException(BeautifyErrorType.ConfigurationError, "Test message")
                .SetComponent("TestComponent");

            // Act
            var result = exception.GetFormattedMessage();

            // Assert
            Assert.Contains("[TestComponent]", result);
            Assert.Contains("[ConfigurationError]", result);
            Assert.Contains("Test message", result);
        }

        [Fact]
        public void GetFormattedMessage_WithContext_ShouldIncludeContext()
        {
            // Arrange
            var exception = new BeautifyException(BeautifyErrorType.ThemeLoadError, "Theme error")
                .SetComponent("ThemeManager")
                .AddContext("ThemeName", "DarkTheme")
                .AddContext("Version", "1.0");

            // Act
            var result = exception.GetFormattedMessage();

            // Assert
            Assert.Contains("[ThemeManager]", result);
            Assert.Contains("[ThemeLoadError]", result);
            Assert.Contains("Theme error", result);
            Assert.Contains("ThemeName=DarkTheme", result);
            Assert.Contains("Version=1.0", result);
        }

        [Fact]
        public void GetUserFriendlyMessage_WithUserMessage_ShouldReturnUserMessage()
        {
            // Arrange
            var userMessage = "User-friendly message";
            var exception = new BeautifyException(BeautifyErrorType.ValidationError, "Technical message", userMessage);

            // Act
            var result = exception.GetUserFriendlyMessage();

            // Assert
            Assert.Equal(userMessage, result);
        }

        [Fact]
        public void GetUserFriendlyMessage_WithoutUserMessage_ShouldReturnTechnicalMessage()
        {
            // Arrange
            var technicalMessage = "Technical message";
            var exception = new BeautifyException(BeautifyErrorType.ValidationError, technicalMessage);
            exception.UserMessage = null; // Explicitly set to null to test fallback

            // Act
            var result = exception.GetUserFriendlyMessage();

            // Assert
            Assert.Equal(technicalMessage, result);
        }

        [Fact]
        public void ConfigurationError_ShouldCreateCorrectException()
        {
            // Arrange
            var message = "Configuration failed";
            var userMessage = "Config error";

            // Act
            var exception = BeautifyException.ConfigurationError(message, userMessage);

            // Assert
            Assert.Equal(BeautifyErrorType.ConfigurationError, exception.ErrorType);
            Assert.Equal(message, exception.Message);
            Assert.Equal(userMessage, exception.UserMessage);
            Assert.Equal("ConfigurationManager", exception.Component);
        }

        [Fact]
        public void ThemeLoadError_ShouldCreateCorrectException()
        {
            // Arrange
            var message = "Theme load failed";
            var themeName = "DarkTheme";
            var userMessage = "Theme error";

            // Act
            var exception = BeautifyException.ThemeLoadError(message, themeName, userMessage);

            // Assert
            Assert.Equal(BeautifyErrorType.ThemeLoadError, exception.ErrorType);
            Assert.Equal(message, exception.Message);
            Assert.Equal(userMessage, exception.UserMessage);
            Assert.Equal("ThemeManager", exception.Component);
            Assert.True(exception.ErrorContext.ContainsKey("ThemeName"));
            Assert.Equal(themeName, exception.ErrorContext["ThemeName"]);
        }

        [Fact]
        public void StyleInjectionError_ShouldCreateCorrectException()
        {
            // Arrange
            var message = "Style injection failed";
            var userMessage = "Style error";

            // Act
            var exception = BeautifyException.StyleInjectionError(message, userMessage);

            // Assert
            Assert.Equal(BeautifyErrorType.StyleInjectionError, exception.ErrorType);
            Assert.Equal(message, exception.Message);
            Assert.Equal(userMessage, exception.UserMessage);
            Assert.Equal("StyleInjector", exception.Component);
        }

        [Fact]
        public void ValidationError_ShouldCreateCorrectException()
        {
            // Arrange
            var message = "Validation failed";
            var fieldName = "ThemeId";
            var userMessage = "Invalid data";

            // Act
            var exception = BeautifyException.ValidationError(message, fieldName, userMessage);

            // Assert
            Assert.Equal(BeautifyErrorType.ValidationError, exception.ErrorType);
            Assert.Equal(message, exception.Message);
            Assert.Equal(userMessage, exception.UserMessage);
            Assert.Equal("Validation", exception.Component);
            Assert.True(exception.ErrorContext.ContainsKey("FieldName"));
            Assert.Equal(fieldName, exception.ErrorContext["FieldName"]);
        }

        [Fact]
        public void MethodChaining_ShouldWorkCorrectly()
        {
            // Act
            var exception = new BeautifyException(BeautifyErrorType.NetworkError, "Network error")
                .SetComponent("NetworkService")
                .AddContext("Url", "http://example.com")
                .AddContext("StatusCode", 404);

            // Assert
            Assert.Equal(BeautifyErrorType.NetworkError, exception.ErrorType);
            Assert.Equal("NetworkService", exception.Component);
            Assert.Equal(2, exception.ErrorContext.Count);
            Assert.Equal("http://example.com", exception.ErrorContext["Url"]);
            Assert.Equal(404, exception.ErrorContext["StatusCode"]);
        }

        [Theory]
        [InlineData(BeautifyErrorType.ConfigurationError)]
        [InlineData(BeautifyErrorType.ThemeLoadError)]
        [InlineData(BeautifyErrorType.StyleInjectionError)]
        [InlineData(BeautifyErrorType.ValidationError)]
        [InlineData(BeautifyErrorType.NetworkError)]
        [InlineData(BeautifyErrorType.CompatibilityError)]
        [InlineData(BeautifyErrorType.FileSystemError)]
        [InlineData(BeautifyErrorType.InitializationError)]
        [InlineData(BeautifyErrorType.UnknownError)]
        public void ErrorType_AllValues_ShouldBeSupported(BeautifyErrorType errorType)
        {
            // Act
            var exception = new BeautifyException(errorType, "Test message");

            // Assert
            Assert.Equal(errorType, exception.ErrorType);
        }

        [Fact]
        public void Timestamp_ShouldBeSetToCurrentTime()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var exception = new BeautifyException();
            var after = DateTime.UtcNow;

            // Assert
            Assert.True(exception.Timestamp >= before);
            Assert.True(exception.Timestamp <= after);
        }
    }
}