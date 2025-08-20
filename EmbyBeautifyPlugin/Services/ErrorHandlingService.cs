using EmbyBeautifyPlugin.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// Service for centralized error handling and logging
    /// </summary>
    public class ErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;
        private readonly Dictionary<BeautifyErrorType, ErrorHandlingStrategy> _errorStrategies;

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorStrategies = InitializeErrorStrategies();
        }

        /// <summary>
        /// Handle a BeautifyException with appropriate logging and recovery strategy
        /// </summary>
        /// <param name="exception">The exception to handle</param>
        /// <param name="context">Additional context information</param>
        /// <returns>Task representing the handling operation</returns>
        public async Task HandleExceptionAsync(BeautifyException exception, Dictionary<string, object> context = null)
        {
            try
            {
                if (exception == null)
                {
                    _logger.LogWarning("Attempted to handle null exception");
                    return;
                }

                // Add additional context if provided
                if (context != null)
                {
                    foreach (var kvp in context)
                    {
                        exception.AddContext(kvp.Key, kvp.Value);
                    }
                }

                // Log the exception based on its severity
                LogException(exception);

                // Apply error handling strategy
                if (_errorStrategies.TryGetValue(exception.ErrorType, out var strategy))
                {
                    await strategy.HandleAsync(exception);
                }
                else
                {
                    await _errorStrategies[BeautifyErrorType.UnknownError].HandleAsync(exception);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogCritical(ex, "Critical error in error handling service");
                }
                catch
                {
                    // If logging fails, we can't do much more - just swallow the exception
                    // to prevent cascading failures
                }
            }
        }

        /// <summary>
        /// Handle a general exception by wrapping it in a BeautifyException
        /// </summary>
        /// <param name="exception">The exception to handle</param>
        /// <param name="errorType">The type of beautify error</param>
        /// <param name="component">The component where the error occurred</param>
        /// <param name="userMessage">User-friendly error message</param>
        /// <param name="context">Additional context information</param>
        /// <returns>Task representing the handling operation</returns>
        public async Task HandleExceptionAsync(Exception exception, BeautifyErrorType errorType, 
            string component = null, string userMessage = null, Dictionary<string, object> context = null)
        {
            var beautifyException = new BeautifyException(errorType, exception.Message, userMessage, exception)
                .SetComponent(component ?? "Unknown");

            if (context != null)
            {
                foreach (var kvp in context)
                {
                    beautifyException.AddContext(kvp.Key, kvp.Value);
                }
            }

            await HandleExceptionAsync(beautifyException);
        }

        /// <summary>
        /// Log an exception with appropriate log level based on error type
        /// </summary>
        /// <param name="exception">The exception to log</param>
        private void LogException(BeautifyException exception)
        {
            var formattedMessage = exception.GetFormattedMessage();
            var logLevel = GetLogLevelForErrorType(exception.ErrorType);

            switch (logLevel)
            {
                case LogLevel.Critical:
                    _logger.LogCritical(exception, formattedMessage);
                    break;
                case LogLevel.Error:
                    _logger.LogError(exception, formattedMessage);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(exception, formattedMessage);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(exception, formattedMessage);
                    break;
                default:
                    _logger.LogDebug(exception, formattedMessage);
                    break;
            }
        }

        /// <summary>
        /// Get the appropriate log level for an error type
        /// </summary>
        /// <param name="errorType">The error type</param>
        /// <returns>The log level</returns>
        private LogLevel GetLogLevelForErrorType(BeautifyErrorType errorType)
        {
            return errorType switch
            {
                BeautifyErrorType.InitializationError => LogLevel.Critical,
                BeautifyErrorType.ConfigurationError => LogLevel.Error,
                BeautifyErrorType.ThemeLoadError => LogLevel.Warning,
                BeautifyErrorType.StyleInjectionError => LogLevel.Warning,
                BeautifyErrorType.ValidationError => LogLevel.Warning,
                BeautifyErrorType.FileSystemError => LogLevel.Error,
                BeautifyErrorType.NetworkError => LogLevel.Warning,
                BeautifyErrorType.CompatibilityError => LogLevel.Information,
                BeautifyErrorType.UnknownError => LogLevel.Error,
                _ => LogLevel.Warning
            };
        }

        /// <summary>
        /// Initialize error handling strategies for different error types
        /// </summary>
        /// <returns>Dictionary of error handling strategies</returns>
        private Dictionary<BeautifyErrorType, ErrorHandlingStrategy> InitializeErrorStrategies()
        {
            return new Dictionary<BeautifyErrorType, ErrorHandlingStrategy>
            {
                { BeautifyErrorType.ConfigurationError, new ConfigurationErrorStrategy(_logger) },
                { BeautifyErrorType.ThemeLoadError, new ThemeLoadErrorStrategy(_logger) },
                { BeautifyErrorType.StyleInjectionError, new StyleInjectionErrorStrategy(_logger) },
                { BeautifyErrorType.ValidationError, new ValidationErrorStrategy(_logger) },
                { BeautifyErrorType.FileSystemError, new FileSystemErrorStrategy(_logger) },
                { BeautifyErrorType.NetworkError, new NetworkErrorStrategy(_logger) },
                { BeautifyErrorType.CompatibilityError, new CompatibilityErrorStrategy(_logger) },
                { BeautifyErrorType.InitializationError, new InitializationErrorStrategy(_logger) },
                { BeautifyErrorType.UnknownError, new UnknownErrorStrategy(_logger) }
            };
        }

        /// <summary>
        /// Create a safe execution wrapper that handles exceptions
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="errorType">The error type if an exception occurs</param>
        /// <param name="component">The component executing the action</param>
        /// <param name="userMessage">User-friendly error message</param>
        /// <returns>Task representing the safe execution</returns>
        public async Task ExecuteSafelyAsync(Func<Task> action, BeautifyErrorType errorType, 
            string component = null, string userMessage = null)
        {
            try
            {
                await action();
            }
            catch (BeautifyException)
            {
                // Re-throw BeautifyExceptions as they are already handled
                throw;
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, errorType, component, userMessage);
                throw; // Re-throw to maintain original behavior
            }
        }

        /// <summary>
        /// Create a safe execution wrapper that handles exceptions and returns a result
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="func">The function to execute</param>
        /// <param name="defaultValue">Default value to return on error</param>
        /// <param name="errorType">The error type if an exception occurs</param>
        /// <param name="component">The component executing the function</param>
        /// <param name="userMessage">User-friendly error message</param>
        /// <returns>Task representing the safe execution with result</returns>
        public async Task<T> ExecuteSafelyAsync<T>(Func<Task<T>> func, T defaultValue, BeautifyErrorType errorType,
            string component = null, string userMessage = null)
        {
            try
            {
                return await func();
            }
            catch (BeautifyException)
            {
                // Re-throw BeautifyExceptions as they are already handled
                throw;
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, errorType, component, userMessage);
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Base class for error handling strategies
    /// </summary>
    public abstract class ErrorHandlingStrategy
    {
        protected readonly ILogger _logger;

        protected ErrorHandlingStrategy(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle the specific error type
        /// </summary>
        /// <param name="exception">The exception to handle</param>
        /// <returns>Task representing the handling operation</returns>
        public abstract Task HandleAsync(BeautifyException exception);
    }

    /// <summary>
    /// Strategy for handling configuration errors
    /// </summary>
    public class ConfigurationErrorStrategy : ErrorHandlingStrategy
    {
        public ConfigurationErrorStrategy(ILogger logger) : base(logger) { }

        public override async Task HandleAsync(BeautifyException exception)
        {
            _logger.LogInformation("Attempting to recover from configuration error");
            
            // Strategy: Try to reset to default configuration
            // This would be implemented when we have the actual configuration manager
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Strategy for handling theme loading errors
    /// </summary>
    public class ThemeLoadErrorStrategy : ErrorHandlingStrategy
    {
        public ThemeLoadErrorStrategy(ILogger logger) : base(logger) { }

        public override async Task HandleAsync(BeautifyException exception)
        {
            _logger.LogInformation("Attempting to recover from theme loading error");
            
            // Strategy: Fall back to default theme
            // This would be implemented when we have the actual theme manager
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Strategy for handling style injection errors
    /// </summary>
    public class StyleInjectionErrorStrategy : ErrorHandlingStrategy
    {
        public StyleInjectionErrorStrategy(ILogger logger) : base(logger) { }

        public override async Task HandleAsync(BeautifyException exception)
        {
            _logger.LogInformation("Attempting to recover from style injection error");
            
            // Strategy: Disable custom styles temporarily
            // This would be implemented when we have the actual style injector
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Strategy for handling validation errors
    /// </summary>
    public class ValidationErrorStrategy : ErrorHandlingStrategy
    {
        public ValidationErrorStrategy(ILogger logger) : base(logger) { }

        public override async Task HandleAsync(BeautifyException exception)
        {
            _logger.LogInformation("Handling validation error");
            
            // Strategy: Log validation details for debugging
            if (exception.ErrorContext.ContainsKey("FieldName"))
            {
                _logger.LogDebug("Validation failed for field: {FieldName}", exception.ErrorContext["FieldName"]);
            }
            
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Strategy for handling file system errors
    /// </summary>
    public class FileSystemErrorStrategy : ErrorHandlingStrategy
    {
        public FileSystemErrorStrategy(ILogger logger) : base(logger) { }

        public override async Task HandleAsync(BeautifyException exception)
        {
            _logger.LogInformation("Attempting to recover from file system error");
            
            // Strategy: Check permissions and retry or use alternative paths
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Strategy for handling network errors
    /// </summary>
    public class NetworkErrorStrategy : ErrorHandlingStrategy
    {
        public NetworkErrorStrategy(ILogger logger) : base(logger) { }

        public override async Task HandleAsync(BeautifyException exception)
        {
            _logger.LogInformation("Handling network error");
            
            // Strategy: Implement retry logic or offline mode
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Strategy for handling compatibility errors
    /// </summary>
    public class CompatibilityErrorStrategy : ErrorHandlingStrategy
    {
        public CompatibilityErrorStrategy(ILogger logger) : base(logger) { }

        public override async Task HandleAsync(BeautifyException exception)
        {
            _logger.LogInformation("Handling compatibility error");
            
            // Strategy: Disable incompatible features or provide alternatives
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Strategy for handling initialization errors
    /// </summary>
    public class InitializationErrorStrategy : ErrorHandlingStrategy
    {
        public InitializationErrorStrategy(ILogger logger) : base(logger) { }

        public override async Task HandleAsync(BeautifyException exception)
        {
            _logger.LogCritical("Critical initialization error occurred");
            
            // Strategy: Disable plugin or enter safe mode
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Strategy for handling unknown errors
    /// </summary>
    public class UnknownErrorStrategy : ErrorHandlingStrategy
    {
        public UnknownErrorStrategy(ILogger logger) : base(logger) { }

        public override async Task HandleAsync(BeautifyException exception)
        {
            _logger.LogError("Unknown error occurred, applying default handling");
            
            // Strategy: Log detailed information and continue with degraded functionality
            await Task.CompletedTask;
        }
    }
}