using Microsoft.Extensions.Logging;

namespace WetterdatenImporter.Logging;

internal sealed class SimpleFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;
    private readonly LogLevel _minimumLogLevel;
    private readonly object _writeLock;

    public SimpleFileLogger(string categoryName, string filePath, LogLevel minimumLogLevel, object writeLock)
    {
        _categoryName = categoryName;
        _filePath = filePath;
        _minimumLogLevel = minimumLogLevel;
        _writeLock = writeLock;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minimumLogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel) == false)
        {
            return;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message) && exception is null)
        {
            return;
        }

        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz} [{logLevel}] {_categoryName}: {message}";
        if (exception is not null)
        {
            line += Environment.NewLine + exception;
        }

        lock (_writeLock)
        {
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
