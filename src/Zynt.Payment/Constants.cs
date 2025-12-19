using System.Text.RegularExpressions;

namespace Zynt.Payment;

internal static partial class Constants
{
    [GeneratedRegex("[a-z]{4,8}")]
    public static partial Regex ValidIdentifierNameRgx { get; }

    public const string HandlerSuffix = "Handler";
}