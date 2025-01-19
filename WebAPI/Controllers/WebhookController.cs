using Domain.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManagementSystemAPI.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhookController : ControllerBase
    {
        private readonly RedisService _redisService;
        private readonly EventBusService _eventBusService;

        public WebhookController(RedisService redisService, EventBusService eventBusService)
        {
            _redisService = redisService;
            _eventBusService = eventBusService;
        }

        [HttpPost]
        public IActionResult CreateWebhook([FromBody] Webhook webhook)
        {
            webhook.Id = Guid.NewGuid();
            _redisService.SaveWebhook(webhook);
            _eventBusService.SubscribeToEvent(webhook.EventName);
            return Ok(webhook);
        }

        [HttpGet("{id}")]
        public IActionResult GetWebhook(Guid id)
        {
            var webhook = _redisService.GetWebhook(id);
            if (webhook == null)
            {
                return NotFound();
            }
            return Ok(webhook);
        }

        [HttpGet("{id}/logs")]
        public IActionResult GetWebhookLogs(Guid id)
        {
            var logs = _redisService.GetWebhookLogs(id);
            return Ok(logs);
        }

        [HttpPost("trigger")]
        public IActionResult TriggerEvent([FromBody] EventData eventData)
        {
            _eventBusService.PublishEvent(eventData.EventName, eventData.Payload);
            return Ok("Event triggered successfully.");
        }
    }
}