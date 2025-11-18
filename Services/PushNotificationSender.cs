using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Data;

namespace WSFBackendApi.Services;

public class PushNotificationSender(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;
    private readonly HttpClient _http = new();

    public async Task SendToAllAsync(string title, string body)
    {
        var tokens = await _context.PushNotificationTokens
            .Where(t => t.EnableOutlineNotifications)
            .Select(t => t.ExpoPushToken)
            .ToListAsync();

        if (tokens.Count == 0)
            return;

        var messages = tokens.Select(token => new
        {
            to = token,
            sound = "default",
            title,
            body,
            priority = "high"
        }).ToList();

        var json = JsonSerializer.Serialize(messages);

        var request = new StringContent(json, Encoding.UTF8, "application/json");

        await _http.PostAsync("https://exp.host/--/api/v2/push/send", request);
    }
}
