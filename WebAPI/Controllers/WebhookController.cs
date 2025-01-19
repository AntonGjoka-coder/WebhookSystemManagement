using Domain.Models;
using Infrastructure.Interfaces.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhookController : ControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly IEventBusService _eventBusService;

        public WebhookController(IRedisService redisService, IEventBusService eventBusService)
        {
            _redisService = redisService;
            _eventBusService = eventBusService;
        }

        [HttpPost]
        public IActionResult CreateWebhook([FromBody] Webhook webhook)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            webhook.Id = Guid.NewGuid();
            
            try
            {
                _redisService.SaveWebhook(webhook);
                _eventBusService.SubscribeToEvent(webhook.EventName);

                return CreatedAtAction(nameof(GetWebhook), new { id = webhook.Id }, webhook);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the webhook.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetWebhook(Guid id)
        {
            try
            {
                var webhook = _redisService.GetWebhook(id);
                if (webhook == null)
                {
                    return NotFound(new { message = "Webhook not found." });
                }

                return Ok(webhook);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the webhook.", details = ex.Message });
            }
        }

        [HttpGet("{id}/logs")]
        public IActionResult GetWebhookLogs(Guid id)
        {
            try
            {
                var logs = _redisService.GetWebhookLogs(id);
                if (logs == null || !logs.Any())
                {
                    return NotFound(new { message = "No logs found for the webhook." });
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving logs.", details = ex.Message });
            }
        }

        [HttpPost("trigger")]
        public IActionResult TriggerEvent([FromBody] EventData eventData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _eventBusService.PublishEvent(eventData.EventName, eventData.Payload);
                return Ok(new { message = "Event triggered successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while triggering the event.", details = ex.Message });
            }
        }
    }
}
