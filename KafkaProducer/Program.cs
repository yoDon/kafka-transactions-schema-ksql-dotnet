using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using KafkaSchema;
using Newtonsoft.Json;

const string topic = "purchases";
const string clientId = "transactional-sample";
const string transactionIdPrefix = "transaction-id";

string[] users = { "alice", "bob", "carol", "dave", "edward", "frank" };
string[] items = { "book", "alarm clock", "t-shirts", "gift card", "batteries" };
string[] colors = { "black", "brown", "red", "orange", "yellow", "green", "blue", "purple", "white" };

var schemaRegistryConfig = new SchemaRegistryConfig
{
    //
    // Note: you can specify more than one schema registry url using the
    // schema.registry.url property for redundancy (comma separated list). 
    // The property name is not plural to follow the convention set by
    // the Java implementation.
    //
    Url = "http://localhost:8081",
};

var jsonSerializerConfig = new JsonSerializerConfig();
using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
using var producer = new ProducerBuilder<string, PurchasedItem>(new ProducerConfig()
    {
        BootstrapServers = "localhost:9092",
        ClientId = clientId + "_producer",
        TransactionalId = transactionIdPrefix + "-" + clientId
    })
    .SetValueSerializer(new JsonSerializer<PurchasedItem>(schemaRegistry, jsonSerializerConfig).AsSyncOverAsync())
    .Build();

TimeSpan defaultTimeout = TimeSpan.FromSeconds(30);
producer.InitTransactions(defaultTimeout);

var numProduced = 0;
var rnd = new Random();
for (var transactionNum = 0; transactionNum < 5; transactionNum++)
{
    //
    // NOTE: In this sample we are just sending messages to a single
    // kafka topic. In production, one commonly wants to send messages
    // to multiple topics as part of a single transaction. The approach
    // used here works equally well for sending messages to multiple topics.
    //

    producer.BeginTransaction();
    try
    {
        for (var itemNumInTransaction = 0; itemNumInTransaction < 2; ++itemNumInTransaction)
        {
            var username = users[rnd.Next(users.Length)];
            var item = new PurchasedItem
            {
                Name = $"{items[rnd.Next(items.Length)]}-{numProduced}",
                Color = colors[rnd.Next(colors.Length)],
                Quantity = 1 + rnd.Next(10),
            };

            producer.Produce(topic, new Message<string, PurchasedItem>
                {
                    Key = username,
                    Value = item
                },
                (deliveryReport) =>
                {
                    if (deliveryReport.Error.Code != ErrorCode.NoError)
                    {
                        Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
                    }
                    else
                    {
                        var text = JsonConvert.SerializeObject(item);
                        Console.WriteLine($"Produced event to topic {topic}: key = {username,-10} value = {text}");
                        numProduced += 1;
                    }
                });
        }
        
        producer.CommitTransaction();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        producer.AbortTransaction();
        throw;
    }
}

//
// NOTE: Flush is not required for transactional producers.
// We are only flushing in case this code gets refactored to
// be non-transactional (in which case adding the Flush might
// get missed and the effects of forgetting to add the flush
// could take a while to notice).
//
producer.Flush(TimeSpan.FromSeconds(10));
Console.WriteLine($"{numProduced} messages were produced to topic {topic}");

