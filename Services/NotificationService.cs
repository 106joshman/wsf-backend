using Microsoft.EntityFrameworkCore;
using WSFBackendApi.Data;
using WSFBackendApi.DTOs;
using WSFBackendApi.Models;

namespace WSFBackendApi.Services;

public class NotificationService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task RegisterToken(RegisterTokenDto registerTokenDto)
    {
        // AVOID DUPLICATES
        var exists = await _context.PushNotificationTokens.FirstOrDefaultAsync(t => t.ExpoPushToken == registerTokenDto.Token);

        if (exists == null)
        {
            var entity = new PushNotificationToken
            {
                ExpoPushToken = registerTokenDto.Token,
                DevicePlatform = registerTokenDto.DevicePlatform,
                EnableOutlineNotifications = registerTokenDto.EnableOutlineNotifications,
                UserId = registerTokenDto.UserId
            };

            await _context.PushNotificationTokens.AddAsync(entity);
        }
        else
        {
            // var token = await _context.PushNotificationTokens.FirstAsync(t => t.ExpoPushToken == registerTokenDto.Token);
            exists.EnableOutlineNotifications = registerTokenDto.EnableOutlineNotifications;

            if (exists.UserId == null && registerTokenDto.UserId != null)
                exists.UserId = registerTokenDto.UserId;
        }
        await _context.SaveChangesAsync();
    }
}