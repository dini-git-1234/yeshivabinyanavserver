using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BinyanAv.PublicGateway.Models;

public class PublicRegistrationDto
{
    /// <summary>es | he — להודעות שגיאה והצלחה.</summary>
    public string? UiLocale { get; set; }

    public string? FirstNameHebrew { get; set; }
    public string? LastNameHebrew { get; set; }
    public string? FirstNameLatin { get; set; }
    public string? LastNameLatin { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MaritalStatus { get; set; }
    public string? CountryOfBirth { get; set; }
    public string? OtherNationality { get; set; }

    public string? PassportNumber { get; set; }
    public string? PassportCountry { get; set; }
    public DateTime? PassportExpiration { get; set; }
    public bool? ParentIsIsraeli { get; set; }
    public bool? HasIsraeliVisa { get; set; }
    public DateTime? VisaExpiration { get; set; }

    public string? AddressStreet { get; set; }
    public string? AddressNumber { get; set; }
    public string? AddressApartment { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressCountry { get; set; }
    public string? AddressZipCode { get; set; }

    public string? PhoneHome { get; set; }
    public string? PhoneMobile { get; set; }
    public string? Email { get; set; }

    public string? FatherNameLatin { get; set; }
    public string? FatherNameHebrew { get; set; }
    public DateTime? FatherDob { get; set; }
    public string? FatherJob { get; set; }
    public string? FatherPhone { get; set; }
    public string? FatherWorkPhone { get; set; }
    public string? FatherEmail { get; set; }

    public string? MotherNameLatin { get; set; }
    public string? MotherNameHebrew { get; set; }
    public string? MotherMaidenName { get; set; }
    public DateTime? MotherDob { get; set; }
    public string? MotherJob { get; set; }
    public string? MotherPhone { get; set; }
    public string? MotherWorkPhone { get; set; }
    public string? MotherEmail { get; set; }

    public string? MaritalStatusParents { get; set; }

    [JsonPropertyName("siblings")]
    public List<PublicSiblingDto>? Siblings { get; set; }

    public string? Community { get; set; }
    public string? PreviousSchool { get; set; }
    public int? PreviousInstitutionGraduationYear { get; set; }
    public string? RabbiName { get; set; }

    public int? IsraelVisitCount { get; set; }
    public string? IsraelLongestStayDescription { get; set; }
    public string? IsraelRelativesNotes { get; set; }

    public DateTime? YeshivaStartDate { get; set; }
    public DateTime? PlannedReturnFromIsrael { get; set; }

    public bool MaccabiEnrollmentAcknowledged { get; set; }
    public string? TuitionPaymentMethod { get; set; }
    public string? TuitionCardholderName { get; set; }
    public string? TuitionCardholderPassportNumber { get; set; }
    public string? TuitionCommitmentSignerName { get; set; }

    /// <summary>אישור קריאת התקנון (חובה כש־UiLocale הוא es).</summary>
    [JsonPropertyName("regulationAcknowledged")]
    public bool RegulationAcknowledged { get; set; }
}

public class PublicSiblingDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }

    [JsonPropertyName("studies")]
    public string? Studies { get; set; }
}

public static class RegistrationValidation
{
    private static readonly Regex HebrewName = new(@"^[\u0590-\u05FF\s'\u05F3\-]{2,80}$", RegexOptions.Compiled);
    private static readonly Regex LatinName = new(@"^[A-Za-zÀ-ÿĀ-ž\s'\-.]{2,80}$", RegexOptions.Compiled);
    private static readonly Regex CountryText = new(@"^[\u0590-\u05FFA-Za-zÀ-ÿĀ-ž\s.\-]{2,80}$", RegexOptions.Compiled);
    private static readonly Regex Passport = new(@"^[A-Z0-9]{6,14}$", RegexOptions.Compiled);
    private static readonly Regex EmailRx = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    public static List<string> Validate(PublicRegistrationDto r)
    {
        var es = string.Equals(r.UiLocale, "es", StringComparison.OrdinalIgnoreCase);
        var errors = new List<string>();

        if (!IsHebrewName(r.FirstNameHebrew))
            errors.Add(es
                ? "Nombre (hebreo): al menos 2 caracteres en hebreo."
                : "שם פרטי בעברית: רק אותיות בעברית (לפחות 2 תווים).");

        if (!IsLatinName(r.FirstNameLatin))
            errors.Add(es
                ? "Nombre (castellano): al menos 2 caracteres latinos."
                : "שם פרטי בלועזית: רק אותיות לטיניות (לפחות 2 תווים).");

        if (!IsLatinName(r.LastNameLatin))
            errors.Add(es
                ? "Apellido (castellano): al menos 2 caracteres latinos."
                : "שם משפחה בלועזית: רק אותיות לטיניות (לפחות 2 תווים).");

        if (r.DateOfBirth == null)
            errors.Add(es ? "Indique la fecha de nacimiento." : "נא לבחור תאריך לידה.");
        else if (r.DateOfBirth >= DateTime.UtcNow.Date)
            errors.Add(es ? "La fecha de nacimiento debe ser en el pasado." : "תאריך לידה חייב להיות בעבר.");
        else
        {
            var age = (DateTime.UtcNow - r.DateOfBirth.Value).TotalDays / 365.25;
            if (age > 120)
                errors.Add(es ? "Fecha de nacimiento no válida." : "תאריך לידה לא סביר.");
        }

        if (string.IsNullOrWhiteSpace(r.CountryOfBirth) || !CountryText.IsMatch(r.CountryOfBirth.Trim()))
            errors.Add(es ? "País de nacimiento: texto válido (mín. 2 caracteres)." : "ארץ לידה: נא למלא טקסט תקין.");

        if (!string.IsNullOrWhiteSpace(r.LastNameHebrew) && !IsHebrewName(r.LastNameHebrew))
            errors.Add(es
                ? "Apellido (hebreo): solo letras hebreas (mín. 2)."
                : "שם משפחה בעברית: רק אותיות בעברית (לפחות 2 תווים).");

        var pass = (r.PassportNumber ?? "").Replace(" ", "").ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(r.PassportNumber) || !Passport.IsMatch(pass))
            errors.Add(es
                ? "Pasaporte: 6–14 caracteres (letras inglesas y números)."
                : "מספר דרכון: 6–14 תווים — אותיות באנגלית וספרות בלבד.");

        if (r.PassportExpiration == null)
            errors.Add(es ? "Indique el vencimiento del pasaporte." : "נא לבחור תוקף דרכון.");

        if (!string.IsNullOrWhiteSpace(r.PassportCountry) && !CountryText.IsMatch(r.PassportCountry.Trim()))
            errors.Add(es ? "Tipo/país de pasaporte: texto válido." : "מדינת הדרכון: נא למלא טקסט תקין.");

        if (r.HasIsraeliVisa == true && r.VisaExpiration == null)
            errors.Add(es ? "Si tiene visa israelí, indique el vencimiento." : "נא לבחור תוקף ויזה.");

        if (!string.IsNullOrWhiteSpace(r.PhoneMobile) && !IsPhone(r.PhoneMobile))
            errors.Add(es ? "Celular: número válido (mín. 9 dígitos)." : "טלפון נייד: נא להזין מספר תקין (לפחות 9 ספרות).");
        if (!string.IsNullOrWhiteSpace(r.PhoneHome) && !IsPhone(r.PhoneHome))
            errors.Add(es ? "Teléfono: número válido." : "טלפון בבית: נא להזין מספר תקין.");
        if (!string.IsNullOrWhiteSpace(r.Email) && !EmailRx.IsMatch(r.Email.Trim()))
            errors.Add(es ? "Correo electrónico no válido." : "דוא״ל: נא להזין כתובת אימייל תקינה.");

        if (!string.IsNullOrWhiteSpace(r.AddressStreet) && r.AddressStreet.Trim().Length < 2)
            errors.Add(es ? "Calle: texto más largo." : "רחוב: נא למלא.");
        if (!string.IsNullOrWhiteSpace(r.AddressCity) && r.AddressCity.Trim().Length < 2)
            errors.Add(es ? "Ciudad: texto válido." : "עיר: נא למלא לפחות 2 תווים.");

        OptionalPersonName(r.FatherNameHebrew, es, "padre (hebreo)", "שם האב בעברית", errors);
        OptionalPersonName(r.FatherNameLatin, es, "padre (castellano)", "שם האב בלועזית", errors);
        OptionalPersonName(r.MotherNameHebrew, es, "madre (hebreo)", "שם האם בעברית", errors);
        OptionalPersonName(r.MotherNameLatin, es, "madre (castellano)", "שם האם בלועזית", errors);

        if (!string.IsNullOrWhiteSpace(r.FatherPhone) && !IsPhone(r.FatherPhone))
            errors.Add(es ? "Teléfono del padre: número válido." : "טלפון האב: נא להזין מספר תקין.");
        if (!string.IsNullOrWhiteSpace(r.MotherPhone) && !IsPhone(r.MotherPhone))
            errors.Add(es ? "Teléfono de la madre: número válido." : "טלפון האם: נא להזין מספר תקין.");

        if (r.PreviousInstitutionGraduationYear is int gy && (gy < 1950 || gy > 2100))
            errors.Add(es ? "Año de graduación no válido." : "שנת סיום מוסד קודם לא סבירה.");

        return errors;
    }

    private static void OptionalPersonName(string? value, bool es, string esLabel, string heLabel, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        var t = value.Trim();
        if (IsHebrewName(t) || IsLatinName(t)) return;
        errors.Add(es ? $"{esLabel}: solo letras hebreas o latinas." : $"{heLabel}: רק אותיות בעברית או לטינית.");
    }

    private static bool IsHebrewName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var t = value.Trim();
        if (t.Length < 2 || !t.Any(c => c >= '\u0590' && c <= '\u05FF')) return false;
        return HebrewName.IsMatch(t);
    }

    private static bool IsLatinName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return LatinName.IsMatch(value.Trim());
    }

    private static bool IsPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length is >= 9 and <= 15;
    }
}
