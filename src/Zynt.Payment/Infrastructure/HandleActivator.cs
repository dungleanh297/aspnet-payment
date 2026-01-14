using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Zynt.Payment.Exceptions;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Infrastructure;

internal sealed class HandleActivator : IPaymentHandleActivator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HandlerRegistry _handlerRegistry;

    public HandleActivator(IServiceProvider serviceProvider, HandlerRegistry handlerRegistry)
    {
        _serviceProvider = serviceProvider;
        _handlerRegistry = handlerRegistry;
    }

    public Task InvokeAsync(PaymentResult result)
    {
        if (!_handlerRegistry.TryGetImplementationInfo(result.HandlerName, out var handlerInfo))
        {
            throw new PaymentHandlerNotFoundException(null, result, $"Cannot find the handler implementation with name '{result.HandlerName}'");
        }

        Task handlerTask = Task.CompletedTask;
        object? handler = null;
        HandlerImplementationTypes implementationTypes = handlerInfo.ImplementationTypes;
        bool isAsyncDisposable = implementationTypes.HasFlag(HandlerImplementationTypes.AsyncDisposable);
        bool isDisposable = implementationTypes.HasFlag(HandlerImplementationTypes.Disposable);

        if (implementationTypes.HasFlag(HandlerImplementationTypes.OnSuccess) && result.Status == PaymentStatus.Success)
        {
            handler = ActivatorUtilities.CreateInstance(_serviceProvider, handlerInfo.Type);
            handlerTask = Unsafe.As<IPaymentSuccessHandler>(handler).OnTransactionSuccessAsync(result);
        }

        if (handlerTask is null)
        {
            return Task.CompletedTask;
        }

        return (isDisposable || isAsyncDisposable) ? AwaitThenDispose(handlerTask, handler!, isAsyncDisposable) : handlerTask;
    }

    private static async Task AwaitThenDispose(Task task, object handler, bool isAsyncDisposable)
    {
        try
        {
            await task;
        }
        finally
        {
            if (isAsyncDisposable)
            {
                await Unsafe.As<IAsyncDisposable>(handler).DisposeAsync();
            }
            else
            {
                Unsafe.As<IDisposable>(handler).Dispose();
            }
        }
    }
}