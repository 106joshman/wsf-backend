using WSFBackendApi.Data;
using WSFBackendApi.DTOs;
using WSFBackendApi.Models;
using Microsoft.EntityFrameworkCore;

namespace WSFBackendApi.Services;

public class OutlineService(ApplicationDbContext context, PushNotificationSender pushSender)
{
    private readonly ApplicationDbContext _context = context;
    private readonly PushNotificationSender _pushSender = pushSender;

    // Existing CreateTeachingAsync method (fixed)
    public async Task<TeachingResponseDto> CreateTeachingAsync(TeachingCreateDto dto, Guid adminId, string adminName)
    {
        var teaching = new Teaching
        {
            Month = dto.Month,
            PropheticFocus = dto.PropheticFocus,
            TeachingSeries = dto.TeachingSeries,
            Scripture = dto.Scripture,
            Introduction = dto.Introduction ?? string.Empty,
            CreatedById = adminId,
            CreatedBy = adminName,
            Weeks = dto.Weeks?.Select(w => new TeachingWeek
            {
                Title = w.Title,
                Subtitle = w.Subtitle ?? string.Empty,
                Content = w.Contents ?? new List<string>()
            }).ToList() ?? new List<TeachingWeek>()
        };

        _context.Teachings.Add(teaching);
        await _context.SaveChangesAsync();

        return new TeachingResponseDto
        {
            Id = teaching.Id,
            Month = teaching.Month,
            PropheticFocus = teaching.PropheticFocus,
            TeachingSeries = teaching.TeachingSeries,
            Scripture = teaching.Scripture,
            Introduction = teaching.Introduction,
            Weeks = dto.Weeks,
            AdminId = teaching.CreatedById,
            AdminName = teaching.CreatedBy,
            CreatedAt = teaching.CreatedAt
        };
    }

    // Prayer Outline Creation
    public async Task<PrayerOutlineResponseDto> CreatePrayerOutlineAsync(PrayerOutlineCreateDto dto, Guid adminId, string adminName)
    {
        var prayerOutline = new PrayerOutline
        {
            Month = dto.Month,
            Title = dto.Title ?? string.Empty,
            CreatedById = adminId,
            CreatedBy = adminName,
            Schedule = dto.Schedule?.Select(s => new PrayerSchedule
            {
                Sn = s.Sn,
                Event = s.Event ?? string.Empty,
                Time = s.Time ?? string.Empty,
                Duration = s.Duration ?? string.Empty,
                Minister = s.Minister ?? string.Empty
            }).ToList() ?? new List<PrayerSchedule>(),
            Prayers = dto.Prayers?.Select(p => new PrayerPoint
            {
                Title = p.Title ?? string.Empty,
                Scripture = p.Scripture ?? string.Empty,
                Verse = p.Verse ?? string.Empty
            }).ToList() ?? new List<PrayerPoint>()
        };

        _context.PrayerOutlines.Add(prayerOutline);
        await _context.SaveChangesAsync();

        return new PrayerOutlineResponseDto
        {
            Id = prayerOutline.Id,
            Month = prayerOutline.Month,
            Title = prayerOutline.Title,
            Schedule = dto.Schedule,
            Prayers = dto.Prayers,
            AdminId = prayerOutline.CreatedById,
            AdminName = prayerOutline.CreatedBy,
            CreatedAt = prayerOutline.CreatedAt
        };
    }

    // Monthly Schedule Creation (combines teaching and prayers)
    public async Task<MonthlyScheduleResponseDto> CreateMonthlyScheduleAsync(MonthlyScheduleCreateDto dto, Guid adminId, string adminName)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            TeachingResponseDto? teachingResponse = null;
            List<PrayerOutlineResponseDto> prayerResponses = [];

            // Create Teaching if provided
            if (dto.Teaching != null)
            {
                teachingResponse = await CreateTeachingAsync(dto.Teaching, adminId, adminName);
            }

            // Create Prayer Outlines if provided
            if (dto.Prayers != null && dto.Prayers.Count != 0)
            {
                foreach (var prayerDto in dto.Prayers)
                {
                    var prayerResponse = await CreatePrayerOutlineAsync(prayerDto, adminId, adminName);
                    prayerResponses.Add(prayerResponse);
                }
            }

            await transaction.CommitAsync();

            await _pushSender.SendToAllAsync(
                "New WSF Monthly Schedule",
                $"{dto?.Teaching?.Month ?? ""} monthly schedule is now available."
            );

            return new MonthlyScheduleResponseDto
            {
                Teaching = teachingResponse,
                Prayers = prayerResponses,
                AdminId = adminId,
                AdminName = adminName,
                CreatedAt = DateTime.UtcNow
            };

        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Get Monthly Schedule by Month
    public async Task<MonthlyScheduleResponseDto?> GetMonthlyScheduleAsync(string month, int year)
    {
        if (String.IsNullOrWhiteSpace(month))
            return null;

        var normalizedMonth = month.Replace("-", " ").Trim();

        var teaching = await _context.Teachings
            .Include(t => t.Weeks)
            .FirstOrDefaultAsync(t => t.Month.ToLower() == normalizedMonth.ToLower() && t.CreatedAt.Year == year);

        var prayers = await _context.PrayerOutlines
            .Include(p => p.Schedule)
            .Include(p => p.Prayers)
            .Where(p => p.Month.ToLower() == normalizedMonth.ToLower() && p.CreatedAt.Year == year)
            .ToListAsync();

        if (teaching == null && !prayers.Any())
            return null;

        TeachingResponseDto? teachingResponse = null;
        if (teaching != null)
        {
            teachingResponse = new TeachingResponseDto
            {
                Id = teaching.Id,
                Month = teaching.Month,
                PropheticFocus = teaching.PropheticFocus,
                TeachingSeries = teaching.TeachingSeries,
                Scripture = teaching.Scripture,
                Introduction = teaching.Introduction,
                Weeks = teaching.Weeks.Select(w => new TeachingWeekDto
                {
                    Title = w.Title,
                    Subtitle = w.Subtitle,
                    Contents = w.Content
                }).ToList(),
                AdminId = teaching.CreatedById,
                AdminName = teaching.CreatedBy,
                CreatedAt = teaching.CreatedAt
            };
        }

        var prayerResponses = prayers.Select(p => new PrayerOutlineResponseDto
        {
            Id = p.Id,
            Month = p.Month,
            Title = p.Title,
            Schedule = p.Schedule.Select(s => new PrayerScheduleDto
            {
                Sn = s.Sn,
                Event = s.Event,
                Time = s.Time,
                Duration = s.Duration,
                Minister = s.Minister
            }).ToList(),
            Prayers = p.Prayers.Select(pr => new PrayerPointDto
            {
                Title = pr.Title,
                Scripture = pr.Scripture,
                Verse = pr.Verse
            }).ToList(),
            AdminId = p.CreatedById,
            AdminName = p.CreatedBy,
            CreatedAt = p.CreatedAt
        }).ToList();

        return new MonthlyScheduleResponseDto
        {
            Teaching = teachingResponse,
            Prayers = prayerResponses,
            AdminId = teaching?.CreatedById ?? prayers.FirstOrDefault()?.CreatedById ?? Guid.Empty,
            AdminName = teaching?.CreatedBy ?? prayers.FirstOrDefault()?.CreatedBy,
            CreatedAt = teaching?.CreatedAt ?? prayers.FirstOrDefault()?.CreatedAt ?? DateTime.UtcNow
        };
    }

    // Get all monthly schedules
    public async Task<List<MonthlyScheduleResponseDto>> GetAllMonthlySchedulesAsync()
    {
        var teachings = await _context.Teachings
            .Include(t => t.Weeks)
            .ToListAsync();

        var prayers = await _context.PrayerOutlines
            .Include(p => p.Schedule)
            .Include(p => p.Prayers)
            .ToListAsync();

        var monthlySchedules = new List<MonthlyScheduleResponseDto>();

        // Group by month
        var allMonths = teachings.Select(t => t.Month)
            .Union(prayers.Select(p => p.Month))
            .Distinct()
            .ToList();

        foreach (var month in allMonths)
        {
            var monthTeaching = teachings.FirstOrDefault(t => t.Month == month);
            var monthPrayers = prayers.Where(p => p.Month == month).ToList();

            TeachingResponseDto? teachingResponse = null;
            if (monthTeaching != null)
            {
                teachingResponse = new TeachingResponseDto
                {
                    Id = monthTeaching.Id,
                    Month = monthTeaching.Month,
                    PropheticFocus = monthTeaching.PropheticFocus,
                    TeachingSeries = monthTeaching.TeachingSeries,
                    Scripture = monthTeaching.Scripture,
                    Introduction = monthTeaching.Introduction,
                    Weeks = monthTeaching.Weeks.Select(w => new TeachingWeekDto
                    {
                        Title = w.Title,
                        Subtitle = w.Subtitle,
                        Contents = w.Content
                    }).ToList(),
                    AdminId = monthTeaching.CreatedById,
                    AdminName = monthTeaching.CreatedBy,
                    CreatedAt = monthTeaching.CreatedAt
                };
            }

            var prayerResponses = monthPrayers.Select(p => new PrayerOutlineResponseDto
            {
                Id = p.Id,
                Month = p.Month,
                Title = p.Title,
                Schedule = p.Schedule.Select(s => new PrayerScheduleDto
                {
                    Sn = s.Sn,
                    Event = s.Event,
                    Time = s.Time,
                    Duration = s.Duration,
                    Minister = s.Minister
                }).ToList(),
                Prayers = p.Prayers.Select(pr => new PrayerPointDto
                {
                    Title = pr.Title,
                    Scripture = pr.Scripture,
                    Verse = pr.Verse
                }).ToList(),
                AdminId = p.CreatedById,
                AdminName = p.CreatedBy,
                CreatedAt = p.CreatedAt
            }).ToList();

            monthlySchedules.Add(new MonthlyScheduleResponseDto
            {
                Teaching = teachingResponse,
                Prayers = prayerResponses,
                AdminId = monthTeaching?.CreatedById ?? monthPrayers.FirstOrDefault()?.CreatedById ?? Guid.Empty,
                AdminName = monthTeaching?.CreatedBy ?? monthPrayers.FirstOrDefault()?.CreatedBy,
                CreatedAt = monthTeaching?.CreatedAt ?? monthPrayers.FirstOrDefault()?.CreatedAt ?? DateTime.UtcNow
            });
        }

        return monthlySchedules;
    }

    public async Task<IEnumerable<MonthlyScheduleResponseDto>> GetSchedulesByYearAsync(int year)
    {
        var teachings = await _context.Teachings
            .Include(t => t.Weeks)
            .Where(t => t.CreatedAt.Year == year)
            .ToListAsync();

        var prayers = await _context.PrayerOutlines
            .Include(p => p.Schedule)
            .Include(p => p.Prayers)
            .Where(p => p.CreatedAt.Year == year)
            .ToListAsync();

        // Map to DTOs
        var results = new List<MonthlyScheduleResponseDto>();

        foreach (var teaching in teachings)
        {
            var relatedPrayers = prayers.Where(p => p.Month.ToLower() == teaching.Month.ToLower()).ToList();

            results.Add(new MonthlyScheduleResponseDto
            {
                Teaching = new TeachingResponseDto
                {
                    Id = teaching.Id,
                    Month = teaching.Month,
                    PropheticFocus = teaching.PropheticFocus,
                    TeachingSeries = teaching.TeachingSeries,
                    Scripture = teaching.Scripture,
                    Introduction = teaching.Introduction,
                    Weeks = teaching.Weeks.Select(w => new TeachingWeekDto
                    {
                        Title = w.Title,
                        Subtitle = w.Subtitle,
                        Contents = w.Content
                    }).ToList(),
                    AdminId = teaching.CreatedById,
                    AdminName = teaching.CreatedBy,
                    CreatedAt = teaching.CreatedAt
                },
                Prayers = relatedPrayers.Select(p => new PrayerOutlineResponseDto
                {
                    Id = p.Id,
                    Month = p.Month,
                    Title = p.Title,
                    Schedule = p.Schedule.Select(s => new PrayerScheduleDto
                    {
                        Sn = s.Sn,
                        Event = s.Event,
                        Time = s.Time,
                        Duration = s.Duration,
                        Minister = s.Minister
                    }).ToList(),
                    Prayers = p.Prayers.Select(pr => new PrayerPointDto
                    {
                        Title = pr.Title,
                        Scripture = pr.Scripture,
                        Verse = pr.Verse
                    }).ToList(),
                    AdminId = p.CreatedById,
                    AdminName = p.CreatedBy,
                    CreatedAt = p.CreatedAt
                }).ToList(),
                AdminId = teaching.CreatedById,
                AdminName = teaching.CreatedBy,
                CreatedAt = teaching.CreatedAt
            });
        }

        return results;
    }
}
