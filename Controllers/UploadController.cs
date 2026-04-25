using Microsoft.AspNetCore.Mvc;

namespace BinyanAv.PublicGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private const long MaxFileBytes = 10 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf" };

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "לא נבחר קובץ" });

        if (file.Length > MaxFileBytes)
            return BadRequest(new { message = "הקובץ גדול מדי (מקסימום 10 מ״ב)." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return BadRequest(new { message = "סוג קובץ לא נתמך. נא להעלות תמונה או PDF." });

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = Guid.NewGuid().ToString() + ext;
        var filePath = Path.Combine(folderPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/uploads/{fileName}";
        return Ok(new { url = imageUrl });
    }
}
