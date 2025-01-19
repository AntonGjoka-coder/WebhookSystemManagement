namespace Doamin.Models;

public class WebhookLog
{
    public DateTime Timestamp { get; set; }
    public string Status { get; set; }
    public int ResponseCode { get; set; }
    public string ResponseBody { get; set; }
}