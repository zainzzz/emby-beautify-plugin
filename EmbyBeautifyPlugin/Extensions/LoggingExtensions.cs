using EmbyBeautifyPlugin.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmbyBeautifyPlugin.Extensions
{
    /// <summary>
    /// Extension methods for enhanced logging functionality
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Log a BeautifyException with structured data
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="exception">The BeautifyException to log</param>
        /// <param name="message">Additional message</param>
        public static void LogBeautifyException(this ILogger logger, BeautifyException exception, string message = null)
        {
            if (logger == null || exception == null) return;

            var logMessage = string.IsNullOrEmpty(message) ? exception.GetFormattedMessage() : $"{message}: {exception.GetFormattedMessage()}";
            var logLevel = GetLogLevelForErrorType(exception.ErrorType);

            using (logger.BeginScope(CreateLogScope(exception)))
            {
                logger.Log(logLevel, exception, logMessage);
            }
        }

        /// <summary>
        /// Log an operation start with context
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operation">The operation name</param>
        /// <param name="context">Additional context</param>
        public static void LogOperationStart(this ILogger logger, string operation, Dictionary<string, object> context = null)
        {
            if (logger == null || string.IsNullOrEmpty(operation)) return;

            var message = $"Starting operation: {operation}";
            if (context != null && context.Any())
            {
                var contextString = string.Join(", ", context.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                message += $" (Context: {contextString})";
            }

            logger.LogDebug(message);
        }

        /// <summary>
        /// Log an operation completion with duration
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operation">The operation name</param>
        /// <param name="duration">The operation duration</param>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="context">Additional context</param>
        public static void LogOperationComplete(this ILogger logger, string operation, TimeSpan duration, 
            bool success = true, Dictionary<string, object> context = null)
        {
            if (logger == null || string.IsNullOrEmpty(operation)) return;

            var status = success ? "completed successfully" : "failed";
            var message = $"Operation {operation} {status} in {duration.TotalMilliseconds:F2}ms";
            
            if (context != null && context.Any())
            {
                var contextString = string.Join(", ", context.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                message += $" (Context: {contextString})";
            }

            var logLevel = success ? LogLevel.Debug : LogLevel.Warning;
            logger.Log(logLevel, message);
        }

        /// <summary>
        /// Log configuration changes
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="configType">The type of configuration</param>
        /// <param name="changes">The changes made</param>
        public static void LogConfigurationChange(this ILogger logger, string configType, Dictionary<string, object> changes)
        {
            if (logger == null || string.IsNullOrEmpty(configType)) return;

            var message = $"Configuration changed: {configType}";
            if (changes != null && changes.Any())
            {
                var changesString = string.Join(", ", changes.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                message += $" (Changes: {changesString})";
            }

            logger.LogInformation(message);
        }

        /// <summary>
        /// Log theme operations
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operation">The theme operation</param>
        /// <param name="themeName">The theme name</param>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="details">Additional details</param>
        public static void LogThemeOperation(this ILogger logger, string operation, string themeName, 
            bool success = true, string details = null)
        {
            if (logger == null || string.IsNullOrEmpty(operation)) return;

            var status = success ? "successful" : "failed";
            var message = $"Theme operation '{operation}' {status}";
            
            if (!string.IsNullOrEmpty(themeName))
            {
                message += $" for theme '{themeName}'";
            }

            if (!string.IsNullOrEmpty(details))
            {
                message += $" - {details}";
            }

            var logLevel = success ? LogLevel.Information : LogLevel.Warning;
            logger.Log(logLevel, message);
        }

        /// <summary>
        /// Log performance metrics
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="metric">The metric name</param>
        /// <param name="value">The metric value</param>
        /// <param name="unit">The unit of measurement</param>
        /// <param name="context">Additional context</param>
        public static void LogPerformanceMetric(this ILogger logger, string metric, double value, 
            string unit = "ms", Dictionary<string, object> context = null)
        {
            if (logger == null || string.IsNullOrEmpty(metric)) return;

            var message = $"Performance metric: {metric} = {value:F2}{unit}";
            if (context != null && context.Any())
            {
                var contextString = string.Join(", ", context.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                message += $" (Context: {contextString})";
            }

            logger.LogDebug(message);
        }

        /// <summary>
        /// Log security-related events
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="eventType">The security event type</param>
        /// <param name="details">Event details</param>
        /// <param name="severity">The severity level</param>
        public static void LogSecurityEvent(this ILogger logger, string eventType, string details, 
            LogLevel severity = LogLevel.Warning)
        {
            if (logger == null || string.IsNullOrEmpty(eventType)) return;

            var message = $"Security event: {eventType}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" - {details}";
            }

            logger.Log(severity, message);
        }

        /// <summary>
        /// Create a structured log scope from a BeautifyException
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns>Dictionary for log scope</returns>
        private static Dictionary<string, object> CreateLogScope(BeautifyException exception)
        {
            var scope = new Dictionary<string, object>
            {
                ["ErrorType"] = exception.ErrorType.ToString(),
                ["Component"] = exception.Component ?? "Unknown",
                ["Timestamp"] = exception.Timestamp,
                ["UserMessage"] = exception.UserMessage ?? string.Empty
            };

            // Add error context
            foreach (var kvp in exception.ErrorContext)
            {
                scope[$"Context.{kvp.Key}"] = kvp.Value;
            }

            return scope;
        }

        /// <summary>
        /// Get the appropriate log level for an error type
        /// </summary>
        /// <param name="errorType">The error type</param>
        /// <returns>The log level</returns>
        private static LogLevel GetLogLevelForErrorType(BeautifyErrorType errorType)
        {
            return errorType switch
            {
                BeautifyErrorType.InitializationError => LogLevel.Critical,
                BeautifyErrorType.ConfigurationError => LogLevel.Error,
                BeautifyErrorType.FileSystemError => LogLevel.Error,
                BeautifyErrorType.ThemeLoadError => LogLevel.Warning,
                BeautifyErrorType.StyleInjectionError => LogLevel.Warning,
                BeautifyErrorType.ValidationError => LogLevel.Warning,
                BeautifyErrorType.NetworkError => LogLevel.Warning,
                BeautifyErrorType.CompatibilityError => LogLevel.Information,
                BeautifyErrorType.UnknownError => LogLevel.Error,
                _ => LogLevel.Warning
            };
        }
    }

    /// <summary>
    /// Disposable class for measuring operation duration
    /// </summary>
    public class OperationTimer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operation;
        private readonly DateTime _startTime;
        private readonly Dictionary<string, object> _context;
        private bool _disposed = false;

        public OperationTimer(ILogger logger, string operation, Dictionary<string, object> context = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _context = context;
            _startTime = DateTime.UtcNow;

            _logger.LogOperationStart(_operation, _context);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                var duration = DateTime.UtcNow - _startTime;
                _logger.LogOperationComplete(_operation, duration, true, _context);
                _disposed = true;
            }
        }

        public void Complete(bool success = true)
        {
            if (!_disposed)
            {
                var duration = DateTime.UtcNow - _startTime;
                _logger.LogOperationComplete(_operation, duration, success, _context);
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Extension methods for creating operation timers
    /// </summary>
    public static class OperationTimerExtensions
    {
        /// <summary>
        /// Create an operation timer for measuring duration
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operation">The operation name</param>
        /// <param name="context">Additional context</param>
        /// <returns>Disposable operation timer</returns>
        public static OperationTimer BeginOperation(this ILogger logger, string operation, 
            Dictionary<string, object> context = null)
        {
            return new OperationTimer(logger, operation, context);
        }
    }
}