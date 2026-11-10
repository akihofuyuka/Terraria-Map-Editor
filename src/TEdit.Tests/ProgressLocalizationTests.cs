using System.Globalization;
using System.Reflection;
using Shouldly;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests;

public class ProgressLocalizationTests
{
    [Fact]
    public void AllProgressKeys_HaveRegisteredLocalizationMapping_UnderZhHans()
    {
        var fields = typeof(ProgressKeys)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .ToList();

        fields.ShouldNotBeEmpty();

        var prevCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("zh-Hans");

            foreach (var field in fields)
            {
                var progressText = (string)field.GetValue(null)!;
                progressText.ShouldNotBeNullOrEmpty();

                var converted = ProgressLocalization.Translate(progressText);

                converted.ShouldNotBeNull($"Progress key '{field.Name}' ('{progressText}') must convert to non-null value");
                var resultStr = converted.ToString();
                resultStr.ShouldNotBeNullOrEmpty($"Progress key '{field.Name}' ('{progressText}') must convert to non-empty string");

                // Critical anti-drift assertion:
                // Under zh-Hans, every mapped key MUST translate to Chinese and NOT fall through to '_ => text'.
                resultStr.ShouldNotBe(progressText,
                    $"Progress key '{field.Name}' ('{progressText}') fell through to default branch without being translated!");
            }
        }
        finally
        {
            CultureInfo.CurrentUICulture = prevCulture;
        }
    }

    [Fact]
    public void UnknownProgressString_FallsBackToOriginalText()
    {
        const string unknown = "Custom Plugin Processing 42%...";
        var result = ProgressLocalization.Translate(unknown);
        result.ShouldBe(unknown);
    }
}
