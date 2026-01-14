using System.Text.RegularExpressions;

namespace Zynt.Payment;

public static partial class Constants
{
    [GeneratedRegex("[a-z\\-]{4,32}")]
    public static partial Regex ValidIdentifierNameRgx { get; }

    public const string HandlerSuffix = "Handler";

    public const string ServiceQueryParameterName = "payservice";
}