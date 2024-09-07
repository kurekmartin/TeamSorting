using Avalonia.Controls.Notifications;

namespace TeamSorting.Models;

public class ReturnMessage(NotificationType notificationType, string message)
{
    public NotificationType NotificationType = notificationType;
    public string Message = message;
}