using System.Text.Json;
using BinyanAv.PublicGateway.Data;
using BinyanAv.PublicGateway.Nedarim;
using Microsoft.AspNetCore.Mvc;

namespace BinyanAv.PublicGateway.Controllers;

/// <summary>Postback מנדרים פלוס: application/json (עסקת אשראי או הקמת הוראת קבע). אין ניסיון חוזר מצד נדרים — יש לשמור כל קריאה.</summary>
[ApiController]
[Route("api/nedarim")]
public class NedarimCallbackController : ControllerBase
{
    private readonly GatewayDbContext _db;

    public NedarimCallbackController(GatewayDbContext db)
    {
        _db = db;
    }

    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromBody] JsonElement body)
    {
        try
        {
            var raw = body.GetRawText();
            if (string.IsNullOrWhiteSpace(raw) || body.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return BadRequest(new { ok = false, error = "empty body" });

            var tx = NedarimJsonIds.TryGetTransactionId(body);
            var keva = NedarimJsonIds.TryGetKevaId(body);

            var row = new InboundNedarimCallback
            {
                ReceivedAtUtc = DateTime.UtcNow,
                TransactionId = tx,
                KevaId = keva,
                PayloadJson = raw
            };
            _db.InboundNedarimCallbacks.Add(row);
            await _db.SaveChangesAsync();

            return Ok(new { ok = true, id = row.Id });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Nedarim callback error]: {ex.Message}");
            return StatusCode(500, new { ok = false });
        }
    }
}
