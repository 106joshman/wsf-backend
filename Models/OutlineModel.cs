using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WSFBackendApi.Models;

// ---------------- TEACHING ----------------
[Index(nameof(Month), IsUnique = true)]
public class Teaching
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Month { get; set; } = string.Empty;

    public string PropheticFocus { get; set; } = string.Empty;

    public string TeachingSeries { get; set; } = string.Empty;

    public string Scripture { get; set; } = string.Empty;

    public string Introduction { get; set; } = string.Empty;

    // Audit fields
    public Guid CreatedById { get; set; }

    public required string CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // NAVIGATION PROPERTY
    public ICollection<TeachingWeek> Weeks { get; set; } = new List<TeachingWeek>();
}

public class TeachingWeek
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TeachingId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public List<string> Content { get; set; } = [];

    public Teaching? Teaching { get; set; }
}

// ---------------- PRAYERS ----------------
public class PrayerOutline
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Month { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;


    // Audit fields
    public Guid CreatedById { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public ICollection<PrayerSchedule> Schedule { get; set; } = [];

    public ICollection<PrayerPoint> Prayers { get; set; } = [];
}

public class PrayerSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PrayerOutlineId { get; set; }

    public PrayerOutline? PrayerOutline { get; set; }

    public int Sn { get; set; }

    public string Event { get; set; } = string.Empty;

    public string Time { get; set; } = string.Empty;

    public string Duration { get; set; } = string.Empty;

    public string Minister { get; set; } = string.Empty;
}

public class PrayerPoint
{
    public Guid Id { get; set; }

    public Guid PrayerOutlineId { get; set; }

    public PrayerOutline? PrayerOutline { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Scripture { get; set; } = string.Empty;

    public string Verse { get; set; } = string.Empty;

}
