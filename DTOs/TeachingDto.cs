using System.ComponentModel.DataAnnotations;

namespace WSFBackendApi.DTOs;

public class TeachingCreateDto
{
    public required string Month { get; set; }

    public required string PropheticFocus { get; set; }

    public required string TeachingSeries { get; set; }

    public required string Scripture { get; set; }

    public string? Introduction { get; set; }

    public List<TeachingWeekDto>? Weeks { get; set; }
}

public class TeachingWeekDto
{
    public required string Title { get; set; }

    public string? Subtitle { get; set; }

    public List<string>? Contents { get; set; }
}

public class TeachingResponseDto : TeachingCreateDto
{
    public Guid Id { get; set; }

    public Guid AdminId { get; set; }

    public string? AdminName { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class MonthlyScheduleCreateDto
{
    public TeachingCreateDto? Teaching { get; set; }

    public List<PrayerOutlineCreateDto>? Prayers { get; set; }
}

public class MonthlyScheduleResponseDto
{
    public TeachingResponseDto? Teaching { get; set; }

    public List<PrayerOutlineResponseDto>? Prayers { get; set; }

    public Guid AdminId { get; set; }

    public string? AdminName { get; set; }

    public DateTime CreatedAt { get; set; }
}