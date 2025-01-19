namespace Infrastructure.Interfaces.Services;

public interface IEventBusService
{
    void SubscribeToEvent(string eventName);
    void PublishEvent(string eventName, object eventData);
}