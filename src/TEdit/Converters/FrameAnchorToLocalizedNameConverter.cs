using System;
using System.Globalization;
using System.Windows.Data;
using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Converters;

/// <summary>
/// Maps a <see cref="FrameAnchor"/> enum value to its localized display name.
/// Names come from the localization data (supplementalNames.{locale}.json "Anchor.*" keys),
/// not from a hardcoded table; the surrounding brackets (used for frame-name suffixes) are trimmed for the dropdown.
/// </summary>
public class FrameAnchorToLocalizedNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FrameAnchor anchor)
            return WorldConfiguration.GetLocalizedAnchor(anchor).Trim('[', ']');

        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
