using System.Text.Json;

namespace BinyanAv.PublicGateway.Nedarim;

/// <summary>מחלץ מזהי עסקה / הוראת קבע מתוך JSON של Callback נדרים פלוס (שמות לפי התיעוד).</summary>
public static class NedarimJsonIds
{
    public static string? TryGetTransactionId(JsonElement root)
    {
        if (TryGetStringAnyCase(root, "TransactionId", "transactionId", out var s)) return s;
        return null;
    }

    public static string? TryGetKevaId(JsonElement root)
    {
        if (TryGetStringAnyCase(root, "KevaId", "kevaId", out var s)) return s;
        return null;
    }

    private static bool TryGetStringAnyCase(JsonElement root, string pascal, string camel, out string? value)
    {
        value = null;
        if (root.TryGetProperty(pascal, out var el))
        {
            value = JsonValueToString(el);
            if (value != null) return true;
        }
        if (root.TryGetProperty(camel, out el))
        {
            value = JsonValueToString(el);
            if (value != null) return true;
        }
        return false;
    }

    private static string? JsonValueToString(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.String => string.IsNullOrWhiteSpace(el.GetString()) ? null : el.GetString(),
            JsonValueKind.Number => el.GetRawText(),
            JsonValueKind.Null => null,
            _ => null
        };
    }
}
