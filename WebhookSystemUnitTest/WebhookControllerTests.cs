using Doamin.Models;
using Domain.Models;
using Infrastructure.Interfaces.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using Xunit;

namespace WebhookSystemUnitTest;

public class WebhookControllerTests
{
 private readonly Mock<IRedisService> _mockRedisService;
    private readonly Mock<IEventBusService> _mockEventBusService;
    private readonly WebhookController _controller;

    public WebhookControllerTests()
    {
        _mockRedisService = new Mock<IRedisService>();
        _mockEventBusService = new Mock<IEventBusService>();
        _controller = new WebhookController(_mockRedisService.Object, _mockEventBusService.Object);
    }

    [Fact]
    public void CreateWebhook_ShouldReturnCreatedResult_WhenValidWebhookIsProvided()
    {
        // Arrange
        var webhook = new Webhook { EventName = "order.created", Url = "https://example.com/webhook" };

        // Act
        var result = _controller.CreateWebhook(webhook) as CreatedAtActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(nameof(_controller.GetWebhook), result.ActionName);
        Assert.IsType<Webhook>(result.Value);
        _mockRedisService.Verify(s => s.SaveWebhook(It.IsAny<Webhook>()), Times.Once);
        _mockEventBusService.Verify(e => e.SubscribeToEvent(webhook.EventName), Times.Once);
    }

    [Fact]
    public void CreateWebhook_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var webhook = new Webhook { EventName = "order.created", Url = "https://example.com/webhook" };
        _mockRedisService.Setup(s => s.SaveWebhook(It.IsAny<Webhook>())).Throws(new Exception("Redis error"));

        // Act
        var result = _controller.CreateWebhook(webhook) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("An error occurred", result.Value.ToString());
    }

    [Fact]
    public void GetWebhook_ShouldReturnOk_WhenWebhookExists()
    {
        // Arrange
        var webhookId = Guid.NewGuid();
        var webhook = new Webhook { Id = webhookId, EventName = "order.created", Url = "https://example.com/webhook" };
        _mockRedisService.Setup(s => s.GetWebhook(webhookId)).Returns(webhook);

        // Act
        var result = _controller.GetWebhook(webhookId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(webhook, result.Value);
    }

    [Fact]
    public void GetWebhook_ShouldReturnNotFound_WhenWebhookDoesNotExist()
    {
        // Arrange
        var webhookId = Guid.NewGuid();
        _mockRedisService.Setup(s => s.GetWebhook(webhookId)).Returns((Webhook)null);

        // Act
        var result = _controller.GetWebhook(webhookId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Webhook not found", result.Value.ToString());
    }

    [Fact]
    public void TriggerEvent_ShouldReturnOk_WhenEventIsTriggered()
    {
        // Arrange
        var eventData = new EventData { EventName = "order.created", Payload = new { OrderId = 123 } };

        // Act
        var result = _controller.TriggerEvent(eventData) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("Event triggered successfully", result.Value.ToString());
        _mockEventBusService.Verify(e => e.PublishEvent(eventData.EventName, eventData.Payload), Times.Once);
    }

    [Fact]
    public void GetWebhookLogs_ShouldReturnOk_WhenLogsExist()
    {
        // Arrange
        var webhookId = Guid.NewGuid();
        var logs = new List<WebhookLog>
        {
            new WebhookLog { Timestamp = DateTime.UtcNow, Status = "success", ResponseCode = 200 },
            new WebhookLog { Timestamp = DateTime.UtcNow, Status = "failed", ResponseCode = 500 }
        };
        _mockRedisService.Setup(s => s.GetWebhookLogs(webhookId)).Returns(logs);

        // Act
        var result = _controller.GetWebhookLogs(webhookId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(logs, result.Value);
    }

    [Fact]
    public void GetWebhookLogs_ShouldReturnNotFound_WhenNoLogsExist()
    {
        // Arrange
        var webhookId = Guid.NewGuid();
        _mockRedisService.Setup(s => s.GetWebhookLogs(webhookId)).Returns(new List<WebhookLog>());

        // Act
        var result = _controller.GetWebhookLogs(webhookId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("No logs found for the webhook", result.Value.ToString());
    }
}