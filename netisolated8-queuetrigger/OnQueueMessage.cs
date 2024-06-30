namespace NetIsolated8QueueTrigger;

using System;
using System.Threading.Tasks;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class OnQueueMessage
{
    private readonly ILogger<OnQueueMessage> _logger;

    public OnQueueMessage(ILogger<OnQueueMessage> logger)
    {
        _logger = logger;
    }

    [Function(nameof(OnQueueMessage))]
    public async Task Run([QueueTrigger("items", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        _logger.LogInformation($"Message received on Azure Storage Queue: '{message.MessageText}'. Processed started.");
        await Task.Delay(200);
        _logger.LogInformation($"Processing of message '{message.MessageText}' complete");
    }
}
