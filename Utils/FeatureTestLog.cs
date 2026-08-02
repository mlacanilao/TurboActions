using System.Globalization;

namespace TurboActions;

internal static class FeatureTestLog
{
    internal static void Log(string feature, string detail)
    {
        TurboActions.LogInfo(message: "[FeatureTest] " + feature + ": " + detail);
    }

    internal static string FormatFloat(float value)
    {
        return value.ToString(provider: CultureInfo.InvariantCulture);
    }
}
