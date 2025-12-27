namespace Zynt.Payment.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class PaymentHandlerAttribute : Attribute
{
    public string Name { get; }

    public PaymentHandlerAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        if (!Constants.ValidIdentifierNameRgx.IsMatch(name))
        {
            throw new ArgumentException($"Invalid payment handler name: {name}. Handler name must contains only lowercase characters and hyphen and its length must be between 4 and 32");
        }

        Name = name;
    }
}