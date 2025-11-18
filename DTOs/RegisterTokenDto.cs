namespace WSFBackendApi.DTOs;
public class RegisterTokenDto
{
    public string Token { get; set; } = default!;
    public Guid? UserId { get; set; } // optional
    public string DevicePlatform { get; set; } = "android";
    public bool EnableOutlineNotifications { get; set; } = true;

}
