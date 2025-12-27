using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Extensions;

namespace Zynt.Payment.Test;

public static class Helpers
{
    public static IServiceCollection CreateServiceCollectionWithPayment()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<HandlerStateCollection>();
        serviceCollection.AddPayment();
        return serviceCollection;
    }
}