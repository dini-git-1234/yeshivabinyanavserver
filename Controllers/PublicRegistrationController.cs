using System.Text.Json;
using BinyanAv.PublicGateway.Data;
using BinyanAv.PublicGateway.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BinyanAv.PublicGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicRegistrationController : ControllerBase
{
    private readonly GatewayDbContext _db;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public PublicRegistrationController(GatewayDbContext db)
    {
        _db = db;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitRegistration([FromBody] PublicRegistrationDto request)
    {
        try
        {
            var errors = RegistrationValidation.Validate(request);
            if (errors.Count > 0)
                return BadRequest(new { success = false, errors });

            if (string.Equals(request.UiLocale, "es", StringComparison.OrdinalIgnoreCase) && !request.RegulationAcknowledged)
                return BadRequest(new
                {
                    success = false,
                    errors = new[] { "Debe marcar que leyó el reglamento en la pestaña «Reglamento». / יש לסמן בלשונית «תקנון» שקראתם את המסמך." }
                });

            var payloadJson = JsonSerializer.Serialize(request, JsonOpts);
            var row = new InboundRegistration
            {
                ReceivedAtUtc = DateTime.UtcNow,
                PayloadJson = payloadJson
            };
            _db.InboundRegistrations.Add(row);
            await _db.SaveChangesAsync();

            var es = string.Equals(request.UiLocale, "es", StringComparison.OrdinalIgnoreCase);
            return Ok(new
            {
                success = true,
                message = es ? "¡Registro recibido correctamente!" : "ההרשמה נקלטה בהצלחה!",
                studentId = row.Id
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Public Gateway Registration Error]: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

            var es = string.Equals(request?.UiLocale, "es", StringComparison.OrdinalIgnoreCase);
            return StatusCode(500, new
            {
                success = false,
                message = es
                    ? "Error interno al guardar. Intente más tarde."
                    : "אירעה שגיאה פנימית בשמירת הנתונים. אנא נסו שוב מאוחר יותר."
            });
        }
    }

    [AllowAnonymous]
    [HttpPost("{registrationId:int}/documents")]
    public async Task<IActionResult> AddDocument(int registrationId, [FromBody] PendingDocumentDto doc)
    {
        var reg = await _db.InboundRegistrations.FindAsync(registrationId);
        if (reg == null) return NotFound();

        var row = new InboundRegistrationDocument
        {
            InboundRegistrationId = registrationId,
            Name = doc.Name,
            FileName = doc.FileName,
            Type = doc.Type,
            Notes = doc.Notes,
            UploadDate = doc.UploadDate == default ? DateTime.UtcNow : doc.UploadDate
        };
        _db.InboundRegistrationDocuments.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new { id = row.Id, row.Name, row.FileName, row.Type, row.Notes, uploadDate = row.UploadDate });
    }

    [AllowAnonymous]
    [HttpPost("{registrationId:int}/photo")]
    public async Task<IActionResult> UploadPhoto(int registrationId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("לא נבחר קובץ");

        var reg = await _db.InboundRegistrations.FindAsync(registrationId);
        if (reg == null) return NotFound();

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"r{registrationId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        reg.PhotoUrl = $"{baseUrl}/uploads/photos/{uniqueFileName}";
        await _db.SaveChangesAsync();

        return Ok(new { photoUrl = reg.PhotoUrl });
    }
}
