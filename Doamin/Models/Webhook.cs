namespace Domain.Models;

public class Webhook
{
    public Guid Id { get; set; }
    public string EventName { get; set; }
    public string Url { get; set; }
    public string AuthType { get; set; }
    public string AuthValue { get; set; }
}