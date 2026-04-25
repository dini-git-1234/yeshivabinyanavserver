using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinyanAv.PublicGateway.Data;

public class InboundRegistrationDocument
{
    public int Id { get; set; }

    public int InboundRegistrationId { get; set; }

    [ForeignKey(nameof(InboundRegistrationId))]
    public InboundRegistration? InboundRegistration { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL יחסי או מלא לאחר העלאה ל-wwwroot/uploads.</summary>
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Type { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    public DateTime UploadDate { get; set; }
}
