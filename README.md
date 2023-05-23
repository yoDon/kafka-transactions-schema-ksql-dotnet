# kafka-transactions-schema-ksql-dotnet

## About

This repo contains a C# dotnet Hello World sample for Kafka including ksql, ksqlDB, schemas, a schema registry, 
and transactions. It also includes the kafka-ui webapp for inspecting the Kafka instance.

The `docker-compose.yml` file will start a local Kafka instance 
with kafka-ui, ksqlDB and a schema-registry.

The `KafkaProducer` project sends random data as a series of transactions to the Kafka topic.

The `KafkaConsumer` project reads data from the Kafka topic.

The `KafkaSchema` project contains a shared data class used by both producer
and consumer projects that is automatically registered as a schema with
the kafka schema registry.

This sample is focused on enabling transactional writes to a Kafka topic,
making sure multiple messages are committed to the Kafka topic 
simultaneously (as a single transaction), or not at all. 

The kafka consumer in this example is intentionally simple and focused entirely
on reading records from the topic to validate that they have been written to Kafka.
The web-based kafka-ui included in the `docker-compose.yml` also makes it easy to
check that records have been written.

If you also want to, for example, transactionally add records to a conventional 
database based on the contents of the records in the Kafka topic, that's possible
to add but this example focuses purely on getting the data into Kafka transactionally
(most of the Kafka transaction examples I found involved tight coupling between writing
to Kafka and writing to a database, which makes the samples hard to understand and
introduces a form of tight coupling that Kafka is designed to help you avoid). 
Once the data has been written to Kafka, one or more consumers can read the data
from Kafka and write it to databases or otherwise consume it, transactionally or not.

## Kafka setup

- `docker-compose up`
- browse to http://localhost:8080 to see the kafka-ui

## ksqlDB setup

- Test the ksql connection in a console/terminal window using the sample ksql code at https://ksqldb.io/quickstart.html

## Running the sample code

- run the `KafkaProducer` C# project
  - browse to http://localhost:8080/ui/clusters/local/all-topics/purchases/messages
  - confirm that messages are being written to the topic
  - note that the transactional handler introduces an invisible offset between each record group
  - in this example pairs of messages are grouped together as a single transaction, so offsets will be 0,1,3,4,6,7,9,10, etc
- run the `KafkaConsumer` C# project
  - see the messages written to the console
  - browse to http://localhost:8080/ui/clusters/local/consumer-groups/kafka-dotnet-getting-started
  - note that the consumer group has read all messages in the topic (0 messages behind)
  - `ctrl-c` to stop the `KafkaConsumer` C# project
