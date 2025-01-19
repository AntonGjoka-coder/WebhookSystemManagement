using Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services;

public class EventBusListenerService : IHostedService
{
    private readonly IEventBusService _eventBusService;

    public EventBusListenerService(IEventBusService eventBusService)
    {
        _eventBusService = eventBusService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _eventBusService.SubscribeToEvent("order.created");
        _eventBusService.SubscribeToEvent("user.updated");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}