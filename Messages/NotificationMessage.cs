using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AgentNovel.Messages;

public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info
}

public sealed class NotificationInfo
{
    public string Message { get; init; } = string.Empty;
    public NotificationType Type { get; init; } = NotificationType.Info;
}

public sealed class NotificationMessage : ValueChangedMessage<NotificationInfo>
{
    public NotificationMessage(NotificationInfo value) : base(value) { }
}
