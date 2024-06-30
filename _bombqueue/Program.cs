using Azure.Messaging.EventHubs;
using System.Text;

Console.WriteLine("=== Queue data publisher ===");

if (args.Length < 2)
{
    Console.WriteLine("Usage: BombQueue queue|eventhub number-of-messages");
    return;
}

int numOfMessages = int.Parse(args[1]);

switch (args[0])
{
    case "queue":
        await BombQueue(numOfMessages);
        break;
    case "eventhub":
        await BombEventHub(numOfMessages);
        break;
    default:
        Console.WriteLine("Unknown publisher");
        break;
}

async Task BombQueue(int num)
{
    Console.WriteLine("* Queue");

    // Connect to Azure Queue
    string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")!;
    string queueName = Environment.GetEnvironmentVariable("QueueName")!;
    var queueClient = new Azure.Storage.Queues.QueueClient(connectionString, queueName);

    Console.WriteLine();
    for (int i = 0; i < num; i++)
    {
        string msg = $"Message_{DateTime.Now:s}_{i}";
        await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(msg)));
        Console.Write(msg + "\r");
    }
}

async Task BombEventHub(int num)
{
    const int BLOCK_SIZE = 70;

    Console.WriteLine("* EventHub");

    // Connect to Azure Event Hub
    string connectionString = Environment.GetEnvironmentVariable("EHConnectionString")!;
    string eventHubName = Environment.GetEnvironmentVariable("EventHubName")!;
    var producer = new Azure.Messaging.EventHubs.Producer.EventHubProducerClient(connectionString, eventHubName);

    Console.WriteLine();
    EventData[] chunk = new EventData[BLOCK_SIZE];
    for (int i = 0; i < num; i++)
    {
        if (i > 0 &&  i % BLOCK_SIZE == 0)
        {
            await producer.SendAsync(chunk);
            Console.WriteLine("Block sent " + i);
        }
        string msg = $"Message_{DateTime.Now:s}_{i}";
        var eventData = new EventData(Encoding.UTF8.GetBytes(msg));
        chunk[i % BLOCK_SIZE] = eventData;
    }
}