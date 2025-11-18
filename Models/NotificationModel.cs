namespace WSFBackendApi.Models;
public class PushNotificationToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExpoPushToken { get; set; } = default!;
    public string DevicePlatform { get; set; } = default!; // ios or android
    public bool EnableOutlineNotifications { get; set; } = true;

    public Guid? UserId { get; set; }   // nullable (guests can have tokens too)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
