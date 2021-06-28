using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace CCube
{
    public class Logger
    {
        private Logger() { }

        public static Logger Service { get; } = new Logger();

        public ObservableCollection<Notification> Notifications { get; } = new ObservableCollection<Notification>();

        public static DateTime ApplicationStartTime { get; } = DateTime.Now;

        public static string LogDirectory
        {
            get
            {
                var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "C³", "Logs");

                if (!Directory.Exists(logDirectory)) Directory.CreateDirectory(logDirectory);

                return logDirectory;
            }
        }
        public static string LogFilePath { get; } = Path.Combine(LogDirectory, $"{ApplicationStartTime.ToString("s").Replace(':', '-')}_input_output_log.csv");
        public static string ImportLogFilePath { get; } = Path.Combine(LogDirectory, $"{ApplicationStartTime.ToString("s").Replace(':', '-')}_import_log.csv");

        public void Clear()
        {
            Notifications.Clear();
        }

        private TextWriter logWriter = null;
        public void Log(Notification notification)
        {
            try
            {
                if (logWriter == null)
                {
                    var logFileExists = File.Exists(LogFilePath);

                    logWriter = TextWriter.Synchronized(new StreamWriter(new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read)));

                    if (!logFileExists)
                    {
                        logWriter.WriteLine("Timestamp,Type,Message");
                    }
                }

                logWriter.WriteLine($"{DateTime.Now:G},{notification.NotificationType},{notification.Message.ToCSV()}");
                logWriter.Flush();
            }

            catch { }

            Notifications.Add(notification);
        }
        public void Log(string message, Notification.NotificationTypes notificationType) { Log(new Notification(message, notificationType)); }

        private TextWriter importLogWriter = null;
        public void LogImport(Input input)
        {
            try
            {
                if (importLogWriter == null)
                {
                    var logFileExists = File.Exists(ImportLogFilePath);
                    
                    importLogWriter = TextWriter.Synchronized(new StreamWriter(new FileStream(ImportLogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read)));

                    if (!logFileExists)
                    {
                        importLogWriter.WriteLine(
                            $"Timestamp," +
                            $"Status," +
                            $"Iteration," +
                            $"Start time," +
                            $"End time," +
                            $"Duration," +
                            $"Message," +
                            $"Name," +
                            $"Project ID," +
                            $"External ID," +
                            $"Execution log," +
                            $"Source file"
                        );
                    }
                }

                var currentIteration = input.CurrentActiveIteration;
                var duration = currentIteration?.Duration;
                var commandParams = input.CCCommandParameters;

                importLogWriter.WriteLine(
                    $"{DateTime.Now:G}," +
                    $"{currentIteration?.CurrentStatus}," +
                    $"{currentIteration?.IterationNumber}," +
                    $"{currentIteration?.StartTime:G}," +
                    $"{currentIteration?.EndTime:G}," +
                    (duration == null ? "" : $"{Math.Floor(duration?.TotalHours ?? 0).ToString("00")}:{duration?.Minutes.ToString("00")}:{duration?.Seconds.ToString("00")}") + "," +
                    $"{currentIteration?.Message?.ToCSV()}," +
                    $"{input.ChunkName?.ToCSV()}," +
                    $"{commandParams?.ProjectId}," +
                    $"{commandParams?.NodeExternalId?.ToCSV()}," +
                    $"{currentIteration?.ExecutionLogString?.ToCSV()}," +
                    $"{input.PathToSourceFile.ToCSV()}"
                );

                importLogWriter.Flush();
            }

            catch { }
        }
    }
}