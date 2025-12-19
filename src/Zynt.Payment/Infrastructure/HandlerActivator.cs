using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Zynt.Payment.Exceptions;
using Zynt.Payment.Registries;

namespace Zynt.Payment;


internal sealed class HandlerActivator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HandlerRegistry _handlerRegistry;

    public HandlerActivator(IServiceProvider serviceProvider, HandlerRegistry handlerRegistry)
    {
        _serviceProvider = serviceProvider;
        _handlerRegistry = handlerRegistry;
    }

    public Task InvokeAsync(PaymentResult result)
    {
        if (!_handlerRegistry.TryGetImplementationInfo(result.HandlerName, out var handlerInfo))
        {
            throw new PaymentHandlerNotFoundException(null, result, $"Cannot find the handler implementation with name \"{result.HandlerName}\"");
        }

        var handler = ActivatorUtilities.CreateInstance(_serviceProvider, handlerInfo.Type);

        Task handlerTask;
        var implementationTypes = handlerInfo.ImplementationTypes;


        var isAsyncDisposable = implementationTypes.HasFlag(HandlerImplementationTypes.AsyncDisposable);
        var isDisposable = implementationTypes.HasFlag(HandlerImplementationTypes.Disposable);

        if (implementationTypes.HasFlag(HandlerImplementationTypes.OnSuccess) && result.Status == PaymentStatus.Success)
        {
            handlerTask = Unsafe.As<IPaymentSuccessHandler>(handler).OnTransactionSuccessAsync(result);
        }
        else
        {
            handlerTask = Task.CompletedTask;
        }

        if (!handlerTask.IsCompleted && (isAsyncDisposable || isDisposable))
        {
            return AwaitThenDispose(handlerTask, handler, isAsyncDisposable);
        }

        return handlerTask;
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