namespace BinyanAv.PublicGateway.Models;

/// <summary>תואם לגוף שמגיע מהקליינט אחרי <see cref="UploadController"/>.</summary>
public class PendingDocumentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
}
