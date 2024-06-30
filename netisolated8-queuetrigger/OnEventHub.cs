namespace NetIsolated8QueueTrigger;

using System;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class OnEventHub
{
    private readonly ILogger<OnEventHub> _logger;

    public OnEventHub(ILogger<OnEventHub> logger)
    {
        _logger = logger;
    }

    [Function(nameof(OnEventHub))]
    public void Run([EventHubTrigger("items", Connection = "EHConnectionString")] EventData[] events)
    {
        _logger.LogInformation("Event Hub batch received: {count} events. Starting processing...", events.Length);
        foreach (EventData @event in events)
        {
            _logger.LogInformation("- Event Body: '{body}'", @event.Body);
            _logger.LogInformation("  Event Content-Type: {contentType}", @event.ContentType);
        }
        _logger.LogInformation("Event Hub batch processing complete.");
    }
}
