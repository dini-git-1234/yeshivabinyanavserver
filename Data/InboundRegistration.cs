using System.ComponentModel.DataAnnotations;

namespace BinyanAv.PublicGateway.Data;

public class InboundRegistration
{
    public int Id { get; set; }

    public DateTime ReceivedAtUtc { get; set; }

    /// <summary>גוף הבקשה כפי שנשלח מהדפדפן (JSON), לאחר אימות.</summary>
    [Required]
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>כתובת תמונת פנים לאחר העלאה (כמו בשרת הראשי).</summary>
    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    public DateTime? ImportedAtUtc { get; set; }

    public List<InboundRegistrationDocument> Documents { get; set; } = new();
}
