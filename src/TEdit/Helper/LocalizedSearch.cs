using System;

namespace TEdit.Helper;

/// <summary>
/// Bilingual search helper: match a query against both the localized display name and the
/// stable English identity name, so a localized UI stays searchable in either language.
/// </summary>
public static class LocalizedSearch
{
    /// <summary>Returns true when <paramref name="query"/> is empty, or matches either name (case-insensitive).</summary>
    public static bool Matches(string? localizedName, string? rawName, string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;
        return Contains(localizedName, query) || Contains(rawName, query);
    }

    private static bool Contains(string? value, string query) =>
        !string.IsNullOrEmpty(value) && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
}
