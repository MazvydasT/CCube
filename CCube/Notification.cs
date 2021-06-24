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
}