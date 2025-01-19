using Doamin.Models;
using Domain.Models;
using Infrastructure.Interfaces.Common;

namespace Infrastructure.Interfaces.Services;

public interface IRedisService
{
    void SaveWebhook(Webhook webhook);
    Webhook GetWebhook(Guid id);
    List<Guid> GetWebhookIdsForEvent(string eventName);
    void SaveWebhookLog(Guid webhookId, WebhookLog log);
    List<WebhookLog> GetWebhookLogs(Guid webhookId);
}