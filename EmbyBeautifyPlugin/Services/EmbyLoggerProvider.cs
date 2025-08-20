using MediaBrowser.Model.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// Logger provider that bridges Emby's logging system with Microsoft.Extensions.Logging
    /// </summary>
    public class EmbyLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
    {
        private readonly ILogManager _logManager;
        private readonly ConcurrentDictionary<string, EmbyLogger> _loggers = new ConcurrentDictionary<string, EmbyLogger>();
        private bool _disposed = false;

        public EmbyLoggerProvider(ILogManager logManager)
        {
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new EmbyLogger(_logManager.GetLogger(name)));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _loggers.Clear();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Logger implementation that wraps Emby's ILogger
    /// </summary>
    internal class EmbyLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly MediaBrowser.Model.Logging.ILogger _embyLogger;

        public EmbyLogger(MediaBrowser.Model.Logging.ILogger embyLogger)
        {
            _embyLogger = embyLogger ?? throw new ArgumentNullException(nameof(embyLogger));
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => true,
                LogLevel.Debug => true,
                LogLevel.Information => true,
                LogLevel.Warning => true,
                LogLevel.Error => true,
                LogLevel.Critical => true,
                _ => false
            };
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    _embyLogger.Debug(message);
                    break;
                case LogLevel.Information:
                    _embyLogger.Info(message);
                    break;
                case LogLevel.Warning:
                    _embyLogger.Warn(message);
                    break;
                case LogLevel.Error:
                    if (exception != null)
                        _embyLogger.ErrorException(message, exception);
                    else
                        _embyLogger.Error(message);
                    break;
                case LogLevel.Critical:
                    if (exception != null)
                        _embyLogger.ErrorException($"CRITICAL: {message}", exception);
                    else
                        _embyLogger.Error($"CRITICAL: {message}");
                    break;
            }
        }
    }
}