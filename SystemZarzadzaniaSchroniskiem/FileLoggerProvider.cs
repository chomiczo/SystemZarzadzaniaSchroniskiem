using Microsoft.Extensions.Logging;

namespace SystemZarzadzaniaSchroniskiem
{

    public class FileLogger : ILogger
    {
        private readonly string _path;
        private readonly object _lock = new();

        public FileLogger(string path)
        {
            _path = path;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel level) => level >= LogLevel.Information;

        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception e, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(level)) return;

            var message = $"{DateTime.Now} {level} {formatter(state, e)}{Environment.NewLine}";

            lock(_lock)
            {
                using (var fileStream = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine(message);
                    }

                }
            }

        }
    }


    public class FileLoggerProvider: ILoggerProvider
    {
        private readonly String _path;

        public FileLoggerProvider(String path)
        {
            _path = path;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_path);
        }

        public void Dispose() { }
    }
}
