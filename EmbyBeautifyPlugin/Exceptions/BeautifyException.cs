using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EmbyBeautifyPlugin.Exceptions
{
    /// <summary>
    /// Error types for beautify plugin operations
    /// </summary>
    public enum BeautifyErrorType
    {
        /// <summary>
        /// Error loading or applying themes
        /// </summary>
        ThemeLoadError,

        /// <summary>
        /// Error injecting styles into the client
        /// </summary>
        StyleInjectionError,

        /// <summary>
        /// Configuration related errors
        /// </summary>
        ConfigurationError,

        /// <summary>
        /// Network or communication errors
        /// </summary>
        NetworkError,

        /// <summary>
        /// Compatibility issues with Emby version or browser
        /// </summary>
        CompatibilityError,

        /// <summary>
        /// File system or I/O related errors
        /// </summary>
        FileSystemError,

        /// <summary>
        /// Validation errors for user input or data
        /// </summary>
        ValidationError,

        /// <summary>
        /// Plugin initialization or lifecycle errors
        /// </summary>
        InitializationError,

        /// <summary>
        /// Unknown or unclassified errors
        /// </summary>
        UnknownError
    }

    /// <summary>
    /// Custom exception class for Emby Beautify Plugin operations
    /// </summary>
    [Serializable]
    public class BeautifyException : Exception
    {
        /// <summary>
        /// The type of error that occurred
        /// </summary>
        public BeautifyErrorType ErrorType { get; set; }

        /// <summary>
        /// User-friendly error message
        /// </summary>
        public string UserMessage { get; set; }

        /// <summary>
        /// Additional context information about the error
        /// </summary>
        public Dictionary<string, object> ErrorContext { get; set; }

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Component or module where the error occurred
        /// </summary>
        public string Component { get; set; }

        /// <summary>
        /// Initializes a new instance of BeautifyException
        /// </summary>
        public BeautifyException() : base()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of BeautifyException with a message
        /// </summary>
        /// <param name="message">The error message</param>
        public BeautifyException(string message) : base(message)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of BeautifyException with a message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public BeautifyException(string message, Exception innerException) : base(message, innerException)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of BeautifyException with error type and message
        /// </summary>
        /// <param name="errorType">The type of error</param>
        /// <param name="message">The error message</param>
        public BeautifyException(BeautifyErrorType errorType, string message) : base(message)
        {
            Initialize();
            ErrorType = errorType;
        }

        /// <summary>
        /// Initializes a new instance of BeautifyException with error type, message and user message
        /// </summary>
        /// <param name="errorType">The type of error</param>
        /// <param name="message">The technical error message</param>
        /// <param name="userMessage">The user-friendly error message</param>
        public BeautifyException(BeautifyErrorType errorType, string message, string userMessage) : base(message)
        {
            Initialize();
            ErrorType = errorType;
            UserMessage = userMessage;
        }

        /// <summary>
        /// Initializes a new instance of BeautifyException with error type, message, user message and inner exception
        /// </summary>
        /// <param name="errorType">The type of error</param>
        /// <param name="message">The technical error message</param>
        /// <param name="userMessage">The user-friendly error message</param>
        /// <param name="innerException">The inner exception</param>
        public BeautifyException(BeautifyErrorType errorType, string message, string userMessage, Exception innerException) 
            : base(message, innerException)
        {
            Initialize();
            ErrorType = errorType;
            UserMessage = userMessage;
        }

        /// <summary>
        /// Initializes a new instance of BeautifyException with error type, message, user message and component
        /// </summary>
        /// <param name="errorType">The type of error</param>
        /// <param name="message">The technical error message</param>
        /// <param name="userMessage">The user-friendly error message</param>
        /// <param name="component">The component where the error occurred</param>
        public BeautifyException(BeautifyErrorType errorType, string message, string userMessage, string component) 
            : base(message)
        {
            Initialize();
            ErrorType = errorType;
            UserMessage = userMessage;
            Component = component;
        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected BeautifyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info != null)
            {
                ErrorType = (BeautifyErrorType)info.GetValue(nameof(ErrorType), typeof(BeautifyErrorType));
                UserMessage = info.GetString(nameof(UserMessage));
                Component = info.GetString(nameof(Component));
                Timestamp = info.GetDateTime(nameof(Timestamp));
                
                // Deserialize ErrorContext
                var contextJson = info.GetString(nameof(ErrorContext));
                if (!string.IsNullOrEmpty(contextJson))
                {
                    try
                    {
                        ErrorContext = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(contextJson);
                    }
                    catch
                    {
                        ErrorContext = new Dictionary<string, object>();
                    }
                }
            }
        }

        /// <summary>
        /// Serialization method
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            
            if (info != null)
            {
                info.AddValue(nameof(ErrorType), ErrorType);
                info.AddValue(nameof(UserMessage), UserMessage);
                info.AddValue(nameof(Component), Component);
                info.AddValue(nameof(Timestamp), Timestamp);
                
                // Serialize ErrorContext
                try
                {
                    var contextJson = System.Text.Json.JsonSerializer.Serialize(ErrorContext);
                    info.AddValue(nameof(ErrorContext), contextJson);
                }
                catch
                {
                    info.AddValue(nameof(ErrorContext), string.Empty);
                }
            }
        }

        /// <summary>
        /// Initialize default values
        /// </summary>
        private void Initialize()
        {
            ErrorType = BeautifyErrorType.UnknownError;
            UserMessage = "An unexpected error occurred in the beautify plugin.";
            ErrorContext = new Dictionary<string, object>();
            Timestamp = DateTime.UtcNow;
            Component = "Unknown";
        }

        /// <summary>
        /// Add context information to the error
        /// </summary>
        /// <param name="key">Context key</param>
        /// <param name="value">Context value</param>
        /// <returns>This exception instance for method chaining</returns>
        public BeautifyException AddContext(string key, object value)
        {
            if (!string.IsNullOrEmpty(key) && value != null)
            {
                ErrorContext[key] = value;
            }
            return this;
        }

        /// <summary>
        /// Set the component where the error occurred
        /// </summary>
        /// <param name="component">Component name</param>
        /// <returns>This exception instance for method chaining</returns>
        public BeautifyException SetComponent(string component)
        {
            Component = component ?? "Unknown";
            return this;
        }

        /// <summary>
        /// Get a formatted error message including context
        /// </summary>
        /// <returns>Formatted error message</returns>
        public string GetFormattedMessage()
        {
            var message = $"[{ErrorType}] {Message}";
            
            if (!string.IsNullOrEmpty(Component))
            {
                message = $"[{Component}] {message}";
            }

            if (ErrorContext.Count > 0)
            {
                var contextItems = new List<string>();
                foreach (var kvp in ErrorContext)
                {
                    contextItems.Add($"{kvp.Key}={kvp.Value}");
                }
                message += $" (Context: {string.Join(", ", contextItems)})";
            }

            return message;
        }

        /// <summary>
        /// Get user-friendly error message or fallback to technical message
        /// </summary>
        /// <returns>User-friendly error message</returns>
        public string GetUserFriendlyMessage()
        {
            return !string.IsNullOrEmpty(UserMessage) ? UserMessage : Message;
        }

        /// <summary>
        /// Create a configuration error
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="userMessage">User-friendly message</param>
        /// <returns>BeautifyException instance</returns>
        public static BeautifyException ConfigurationError(string message, string userMessage = null)
        {
            return new BeautifyException(BeautifyErrorType.ConfigurationError, message, 
                userMessage ?? "There was a problem with the plugin configuration.")
                .SetComponent("ConfigurationManager");
        }

        /// <summary>
        /// Create a theme loading error
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="themeName">Name of the theme that failed to load</param>
        /// <param name="userMessage">User-friendly message</param>
        /// <returns>BeautifyException instance</returns>
        public static BeautifyException ThemeLoadError(string message, string themeName = null, string userMessage = null)
        {
            var exception = new BeautifyException(BeautifyErrorType.ThemeLoadError, message,
                userMessage ?? "Failed to load the selected theme.")
                .SetComponent("ThemeManager");

            if (!string.IsNullOrEmpty(themeName))
            {
                exception.AddContext("ThemeName", themeName);
            }

            return exception;
        }

        /// <summary>
        /// Create a style injection error
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="userMessage">User-friendly message</param>
        /// <returns>BeautifyException instance</returns>
        public static BeautifyException StyleInjectionError(string message, string userMessage = null)
        {
            return new BeautifyException(BeautifyErrorType.StyleInjectionError, message,
                userMessage ?? "Failed to apply custom styles to the interface.")
                .SetComponent("StyleInjector");
        }

        /// <summary>
        /// Create a validation error
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="fieldName">Name of the field that failed validation</param>
        /// <param name="userMessage">User-friendly message</param>
        /// <returns>BeautifyException instance</returns>
        public static BeautifyException ValidationError(string message, string fieldName = null, string userMessage = null)
        {
            var exception = new BeautifyException(BeautifyErrorType.ValidationError, message,
                userMessage ?? "The provided data is not valid.")
                .SetComponent("Validation");

            if (!string.IsNullOrEmpty(fieldName))
            {
                exception.AddContext("FieldName", fieldName);
            }

            return exception;
        }
    }
}