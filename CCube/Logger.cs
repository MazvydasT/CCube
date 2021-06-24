using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace CCube
{
    public class Logger : IDisposable
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
        public static string LogFilePath { get; } = Path.Combine(LogDirectory, $"{ApplicationStartTime.ToString("s").Replace(':', '-')}_input_log.csv");
        public static string ImportLogFilePath { get; } = Path.Combine(LogDirectory, $"{ApplicationStartTime.ToString("s").Replace(':', '-')}_import_log.csv");

        public void Clear()
        {
            Notifications.Clear();
        }

        private StreamWriter logWriter = null;
        public void Log(Notification notification)
        {
            try
            {
                if (logWriter == null)
                {
                    var logFileExists = File.Exists(LogFilePath);

                    logWriter = new StreamWriter(LogFilePath, true);

                    if (!logFileExists)
                    {
                        logWriter.WriteLine("Timestamp,Type,Message");
                    }
                }
                
                logWriter.WriteLine($"{DateTime.Now:G},{notification.NotificationType},{notification.Message.ToCSV()}");
            }

            catch { }

            Notifications.Add(notification);
        }
        public void Log(string message, Notification.NotificationTypes notificationType) { Log(new Notification(message, notificationType)); }

        private StreamWriter importLogWriter = null;
        public void LogImport(Input input)
        {
            try
            {
                if(importLogWriter == null)
                {
                    var logFileExists = File.Exists(ImportLogFilePath);

                    importLogWriter = new StreamWriter(ImportLogFilePath, true);

                    if(!logFileExists)
                    {
                        importLogWriter.WriteLine(
                            $"Timestamp," +
                            $"State," +
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

                importLogWriter.WriteLine(
                    $"{DateTime.Now:G}," +
                    $"{input.LatestIteration?.CurrentStatus}," +
                    $"{input.LatestIteration?.StartTime:G}," +
                    $"{input.LatestIteration?.EndTime:G}," +
                    $"{input.LatestIteration?.Duration?.ToString("0:hh':'mm':'ss")}," +
                    $"{input.LatestIteration?.Message?.ToCSV()}," +
                    $"{input.ChunkName?.ToCSV()}," +
                    $"{input.CCCommandParameters.ProjectId}," +
                    $"{input.CCCommandParameters.NodeExternalId?.ToCSV()}," +
                    $"{input.LatestIteration?.ExecutionLogString?.ToCSV()}," +
                    $"{input.PathToSourceFile}"
                );
            }

            catch { }
        }

        public void Dispose()
        {
            logWriter?.Dispose();
            importLogWriter?.Dispose();
        }
    }
}