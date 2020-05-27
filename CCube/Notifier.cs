using System.Collections.ObjectModel;

namespace CCube
{
    public class Notification
    {
        public enum NotificationTypes
        {
            Information,
            Warning,
            Error
        }
        public string Message { get; private set; }
        public NotificationTypes NotificationType { get; private set; }

        public Notification(string message, NotificationTypes notificationType)
        {
            Message = message;
            NotificationType = notificationType;
        }
    }
    public class Notifier
    {
        private Notifier() { }
        public static Notifier Service { get; } = new Notifier();

        public ObservableCollection<Notification> Notifications { get; } = new ObservableCollection<Notification>();

        public void Clear()
        {
            Notifications.Clear();
        }

        public void Log(Notification notification) { Notifications.Add(notification); }
        public void Log(string message, Notification.NotificationTypes notificationType) { Log(new Notification(message, notificationType)); }
    }
}