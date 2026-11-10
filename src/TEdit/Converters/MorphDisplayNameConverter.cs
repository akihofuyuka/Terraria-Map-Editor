using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TEdit.Properties;

namespace TEdit.Converters;

/// <summary>
/// Converts biome identifiers to localized display names using standard resource keys.
/// </summary>
public class MorphBiomeDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string biomeName)
        {
            return biomeName switch
            {
                "Purify" => Language.MorphBiome_Purify,
                "Corruption" => Language.MorphBiome_Corruption,
                "Crimson" => Language.MorphBiome_Crimson,
                "Hallow" => Language.MorphBiome_Hallow,
                "Mushroom" or "GlowingMushroom" => Language.MorphBiome_Mushroom,
                "Jungle" => Language.MorphBiome_Jungle,
                "Forest" => Language.MorphBiome_Forest,
                "Snow" => Language.MorphBiome_Snow,
                "Desert" => Language.MorphBiome_Desert,
                _ => biomeName
            };
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

/// <summary>
/// Converts moss type names to localized display names using standard resource keys.
/// </summary>
public class MorphMossDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string mossName)
        {
            return mossName switch
            {
                "None (Remove)" => Language.morph_moss_none,
                "Grass" => Language.morph_moss_grass,
                "Green" => Language.morph_moss_green,
                "Yellow" => Language.morph_moss_yellow,
                "Red" => Language.morph_moss_red,
                "Blue" => Language.morph_moss_blue,
                "Purple" => Language.morph_moss_purple,
                "Lava" => Language.morph_moss_lava,
                "Krypton" => Language.morph_moss_krypton,
                "Xenon" => Language.morph_moss_xenon,
                "Argon" => Language.morph_moss_argon,
                "Neon" => Language.morph_moss_neon,
                "Helium" => Language.morph_moss_helium,
                _ => mossName
            };
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
