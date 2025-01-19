using System.Text;
using Domain.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net.Http;
using Doamin.Models;
using Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class EventBusService : IEventBusService
    {
        private readonly IRedisService _redisService;
        private readonly IConnectionMultiplexer _connection;

        public EventBusService(IRedisService redisService, IConfiguration configuration)
        {
            _redisService = redisService;
            _connection = ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection"));
        }

        public void SubscribeToEvent(string eventName)
        {
            var subscriber = _connection.GetSubscriber();
            subscriber.Subscribe(eventName, (channel, message) =>
            {
                var eventData = JsonConvert.DeserializeObject<dynamic>(message);
                NotifyWebhooks(eventName, eventData);
            });
        }

        public void PublishEvent(string eventName, object eventData)
        {
            var publisher = _connection.GetSubscriber();
            var eventJson = JsonConvert.SerializeObject(eventData);
            publisher.Publish(eventName, eventJson);
        }

        private void NotifyWebhooks(string eventName, dynamic eventData)
        {
            var webhooks = GetWebhooksForEvent(eventName);
            foreach (var webhook in webhooks)
            {
                SendEventToWebhook(webhook, eventData);
            }
        }

        private List<Webhook> GetWebhooksForEvent(string eventName)
        {
            var webhookIds = _redisService.GetWebhookIdsForEvent(eventName);
            return webhookIds.Select(id => _redisService.GetWebhook(id)).Where(webhook => webhook != null).ToList();
        }

        private async Task SendEventToWebhook(Webhook webhook, dynamic eventData)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(eventData), Encoding.UTF8, "application/json")
            };

            if (webhook.AuthType == "Bearer")
            {
                request.Headers.Add("Authorization", $"Bearer {webhook.AuthValue}");
            }
            else if (webhook.AuthType == "Basic")
            {
                request.Headers.Add("Authorization", $"Basic {webhook.AuthValue}");
            }

            try
            {
                var response = await client.SendAsync(request);
                var log = new WebhookLog
                {
                    Timestamp = DateTime.UtcNow,
                    Status = response.IsSuccessStatusCode ? "success" : "failed",
                    ResponseCode = (int)response.StatusCode,
                    ResponseBody = await response.Content.ReadAsStringAsync()
                };
                _redisService.SaveWebhookLog(webhook.Id, log);
            }
            catch (Exception ex)
            {
                var log = new WebhookLog
                {
                    Timestamp = DateTime.UtcNow,
                    Status = "failed",
                    ResponseCode = 500,
                    ResponseBody = ex.Message
                };
                _redisService.SaveWebhookLog(webhook.Id, log);
            }
        }
    }
}
