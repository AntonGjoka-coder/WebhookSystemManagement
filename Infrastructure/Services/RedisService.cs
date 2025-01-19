using Doamin.Models;
using StackExchange.Redis;
using Domain.Models;
using Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Infrastructure.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _connection;

        public RedisService(IConfiguration configuration)
        {
            _connection = ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection"));
        }

        public void SaveWebhook(Webhook webhook)
        {
            var db = _connection.GetDatabase();
            // Convert Guid to string when saving in Redis
            db.StringSet($"webhook:{webhook.Id}", JsonConvert.SerializeObject(webhook));
            db.SetAdd("webhooks", webhook.Id.ToString());
        }

        public Webhook GetWebhook(Guid id)
        {
            var db = _connection.GetDatabase();
            var webhookJson = db.StringGet($"webhook:{id}");
            return webhookJson.HasValue ? JsonConvert.DeserializeObject<Webhook>(webhookJson) : null;
        }

        public List<Guid> GetWebhookIdsForEvent(string eventName)
        {
            var db = _connection.GetDatabase();
            var webhookIds = db.SetMembers($"webhooks:{eventName}");
            return webhookIds.Select(id => Guid.Parse(id)).ToList();  // Convert string to Guid
        }

        public void SaveWebhookLog(Guid webhookId, WebhookLog log)
        {
            var db = _connection.GetDatabase();
            db.ListLeftPush($"webhookLogs:{webhookId}", JsonConvert.SerializeObject(log));
        }

        public List<WebhookLog> GetWebhookLogs(Guid webhookId)
        {
            var db = _connection.GetDatabase();
            var logEntries = db.ListRange($"webhookLogs:{webhookId}");
            return logEntries.Select(entry => JsonConvert.DeserializeObject<WebhookLog>(entry)).ToList();
        }
    }
}