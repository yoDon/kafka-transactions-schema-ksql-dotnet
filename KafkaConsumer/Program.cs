using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;
using KafkaSchema;
using Newtonsoft.Json;

const string topic = "purchases";
const string clientId = "transactional-sample";

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => {
    e.Cancel = true; // prevent the process from terminating.
    cts.Cancel();
};

using var consumer = new ConsumerBuilder<string, PurchasedItem>(new ConsumerConfig()
    {
        BootstrapServers = "localhost:9092",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        ClientId = clientId + "-consumer",
        GroupId = "kafka-dotnet-getting-started",
        // CooperativeSticky assignment strategy avoids stop-the-world re-balances
        PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
    })
    .SetKeyDeserializer(Deserializers.Utf8)
    .SetValueDeserializer(new JsonDeserializer<PurchasedItem>().AsSyncOverAsync())
    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
    .Build();

consumer.Subscribe(topic);
try
{
    while (true)
    {
        var message = consumer.Consume(cts.Token).Message;
        var username = message.Key;
        var item = message.Value;
        var text = JsonConvert.SerializeObject(item);
        Console.WriteLine(
            $"Consumed event from topic {topic} for user {message.Key} and value {text}");
    }
}
catch (OperationCanceledException)
{
    // Ctrl-C was pressed.
}
finally
{
    consumer.Close();
}

