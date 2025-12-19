namespace Zynt.Payment;

[AttributeUsage(AttributeTargets.Class)]
public class PaymentHandlerAttribute : Attribute
{
    public string Name { get; }

    public PaymentHandlerAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        if (!Constants.ValidIdentifierNameRgx.IsMatch(name))
        {
            throw new ArgumentException($"Invalid payment handler name: {name}. Handler name must consist of 4 to 8 lowercase letters.");
        }

        Name = name;
    }
}