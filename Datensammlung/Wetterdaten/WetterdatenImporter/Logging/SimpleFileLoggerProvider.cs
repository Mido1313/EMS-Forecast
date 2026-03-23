using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace WetterdatenImporter.Logging;

public sealed class SimpleFileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly LogLevel _minimumLogLevel;
    private readonly ConcurrentDictionary<string, SimpleFileLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _writeLock = new();

    public SimpleFileLoggerProvider(string filePath, LogLevel minimumLogLevel = LogLevel.Information)
    {
        _filePath = filePath;
        _minimumLogLevel = minimumLogLevel;

        var directory = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrWhiteSpace(directory) == false)
        {
            Directory.CreateDirectory(directory);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new SimpleFileLogger(name, _filePath, _minimumLogLevel, _writeLock));
    }

    public void Dispose()
    {
    }
}
