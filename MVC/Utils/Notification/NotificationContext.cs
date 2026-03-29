namespace API.Utils.Notification;

public class NotificationContext
{
    public readonly List<Notification> _notifications = [];

    public bool HasNotifications => _notifications.Count != 0;

    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    public void AddNotification(string message, ErrorType errorCode)
    {
        _notifications.Add(new Notification(message, errorCode));
    }
}