namespace CommonGround.SharedKernel.Localization;

/// <summary>
/// The set of locales the app serves content in, and resolution of a requested locale
/// to a supported one. English ("en") is the default and the field-level fallback.
/// A null, empty, or unsupported request resolves to the default.
/// </summary>
public static class SupportedLocales
{
    /// <summary>Default and fallback locale.</summary>
    public const string Default = "en";

    private static readonly HashSet<string> Codes =
        new(StringComparer.OrdinalIgnoreCase) { "en", "de" };

    /// <summary>All supported locale codes.</summary>
    public static IReadOnlyCollection<string> All => Codes;

    /// <summary>True if <paramref name="locale"/> is a supported code (case-insensitive).</summary>
    public static bool IsSupported(string? locale) =>
        !string.IsNullOrWhiteSpace(locale) && Codes.Contains(locale.Trim());

    /// <summary>
    /// Resolves a requested locale to a supported, lower-cased code. Returns
    /// <see cref="Default"/> when the request is null, empty, or unsupported.
    /// </summary>
    public static string Resolve(string? requested)
    {
        if (string.IsNullOrWhiteSpace(requested))
            return Default;

        var normalised = requested.Trim().ToLowerInvariant();
        return Codes.Contains(normalised) ? normalised : Default;
    }
}
