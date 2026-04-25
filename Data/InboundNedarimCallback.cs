using System.ComponentModel.DataAnnotations;

namespace BinyanAv.PublicGateway.Data;

/// <summary>עסקת אשראי או הקמת הוראת קבע — JSON גולמי מתיעוד Callback של נדרים פלוס.</summary>
public class InboundNedarimCallback
{
    public int Id { get; set; }

    public DateTime ReceivedAtUtc { get; set; }

    /// <summary>מזהה עסקה בנדרים (אם קיים).</summary>
    public string? TransactionId { get; set; }

    /// <summary>מזהה הוראת קבע (אם זו הקמת קבע).</summary>
    public string? KevaId { get; set; }

    [Required]
    public string PayloadJson { get; set; } = string.Empty;

    public DateTime? ImportedAtUtc { get; set; }
}
