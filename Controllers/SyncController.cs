using BinyanAv.PublicGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinyanAv.PublicGateway.Controllers;

/// <summary>לשרת הפנימי — משיכת רשומות ממתינות לאחר שמירה במערכת המרכזית.</summary>
[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly GatewayDbContext _db;
    private readonly IConfiguration _cfg;

    public SyncController(GatewayDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    [HttpGet("registrations")]
    public async Task<IActionResult> PendingRegistrations([FromHeader(Name = "X-Sync-Key")] string? key)
    {
        if (!IsValidKey(key)) return Unauthorized();
        var rows = await _db.InboundRegistrations
            .AsNoTracking()
            .Where(x => x.ImportedAtUtc == null)
            .OrderBy(x => x.Id)
            .Take(200)
            .Select(r => new
            {
                r.Id,
                r.ReceivedAtUtc,
                r.PayloadJson,
                r.PhotoUrl,
                documents = r.Documents.Select(d => new
                {
                    d.Name,
                    d.FileName,
                    d.Type,
                    d.Notes,
                    d.UploadDate
                })
            })
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("registrations/ack")]
    public async Task<IActionResult> AckRegistrations([FromHeader(Name = "X-Sync-Key")] string? key, [FromBody] AckBatchDto? body)
    {
        if (!IsValidKey(key)) return Unauthorized();
        if (body?.Ids == null || body.Ids.Length == 0) return BadRequest(new { error = "ids required" });

        // דרישת מוצר: לא לשמור נתונים בשרת החיצוני מעבר לנדרש.
        // מוחקים רק אחרי ACK מהשרת הפנימי (כלומר אחרי שנשמר בהצלחה).
        var rows = await _db.InboundRegistrations
            .Include(x => x.Documents)
            .Where(x => body.Ids.Contains(x.Id))
            .ToListAsync();

        foreach (var r in rows)
        {
            TryDeleteLocalUploads(r.PhotoUrl);
            foreach (var d in r.Documents)
                TryDeleteLocalUploads(d.FileName);
        }

        _db.InboundRegistrations.RemoveRange(rows);
        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }

    [HttpGet("nedarim")]
    public async Task<IActionResult> PendingNedarim([FromHeader(Name = "X-Sync-Key")] string? key)
    {
        if (!IsValidKey(key)) return Unauthorized();
        var rows = await _db.InboundNedarimCallbacks
            .AsNoTracking()
            .Where(x => x.ImportedAtUtc == null)
            .OrderBy(x => x.Id)
            .Take(200)
            .Select(x => new { x.Id, x.ReceivedAtUtc, x.TransactionId, x.KevaId, x.PayloadJson })
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("nedarim/ack")]
    public async Task<IActionResult> AckNedarim([FromHeader(Name = "X-Sync-Key")] string? key, [FromBody] AckBatchDto? body)
    {
        if (!IsValidKey(key)) return Unauthorized();
        if (body?.Ids == null || body.Ids.Length == 0) return BadRequest(new { error = "ids required" });

        var rows = await _db.InboundNedarimCallbacks
            .Where(x => body.Ids.Contains(x.Id))
            .ToListAsync();

        _db.InboundNedarimCallbacks.RemoveRange(rows);
        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }

    private void TryDeleteLocalUploads(string? urlOrPath)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath)) return;

        try
        {
            // תומך גם ב-URL מלא וגם בנתיב יחסי /uploads/...
            var s = urlOrPath.Trim();
            if (Uri.TryCreate(s, UriKind.Absolute, out var abs))
                s = abs.AbsolutePath;

            if (!s.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                return;

            var rel = s.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var full = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", rel["uploads".Length..].TrimStart(Path.DirectorySeparatorChar));
            // למעלה קצת שמרני מדי; נבנה נכון:
            full = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", rel);
            if (System.IO.File.Exists(full))
                System.IO.File.Delete(full);
        }
        catch
        {
            // לא מפילים ACK בגלל כשל מחיקה; במקרה הגרוע נשאר קובץ orphan.
        }
    }

    private bool IsValidKey(string? key)
    {
        var expected = _cfg["Sync:ApiKey"];
        return !string.IsNullOrEmpty(expected) && string.Equals(key, expected, StringComparison.Ordinal);
    }
}

public class AckBatchDto
{
    public int[] Ids { get; set; } = Array.Empty<int>();
}
