namespace WSFBackendApi.DTOs;

public class PrayerOutlineCreateDto
{
    public required string Month { get; set; }
    public string? Title { get; set; }
    public List<PrayerScheduleDto>? Schedule { get; set; }
    public List<PrayerPointDto>? Prayers { get; set; }
}

public class PrayerScheduleDto
{
    public int Sn { get; set; }
    public string? Event { get; set; }
    public string? Time { get; set; }
    public string? Duration { get; set; }
    public string? Minister { get; set; }
}

public class PrayerPointDto
{
    public string? Title { get; set; }
    public string? Scripture { get; set; }
    public string? Verse { get; set; }
}

public class PrayerOutlineResponseDto : PrayerOutlineCreateDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AdminId { get; set; }
    public string? AdminName { get; set; }
    public DateTime CreatedAt { get; set; }
}
