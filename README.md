# kafka-playground


Includes many aspects of the Kafka world from a .Net perspective :

## Confluent .Net Client
Confluent.Kafka nuget package to create Producer and Consumer in the folder Kafka-simple-producer-consumer:
- Simple-Producer
- Simple-Consumer

## Streamiz Kafka .NET
Streamiz Kafka .NET nuget package to create a Stream Client in the folder Kafka-streaming-WikiEdit:
- WikiEditStream 
- WikiEditStream.Tests
- Reference:
	- https://github.com/LGouellec/kafka-streams-dotnet

- This project also includes a .Net TestContainer Setup for Confluent Docker Compose

## ksqlDb:
Confluent ksqlDb for .Net in the folder Kafka-KsqlDb:

- KsqlDb using ksqlDB.RestApi.Client and Confluent.Kafka nuget packages.
- A forked version of ksqlDB.RestApi.Client in the solution ksqlDB.RestApi.Client-Forked

Test integration in the project KsqlDb/KsqlDb.Tests of
- Kafka Producer and Consumer, and Streaming using .Net Clients
- KsqlDb using .Net Clients
- Using TestContainers.Net to create docker instances similar to docker-compose for Kafka

## KafkaFlow
Farfetch KafkaFlow in an adapted sample, in the Kafka-KafkaFlow folder
- Consumer
- Producer
- FlowControl
- References:
	- https://github.com/farfetch
	- https://farfetch.github.io/kafkaflow/
	- https://www.infoq.com/articles/kafkaflow-dotnet-framework/?itm_source=infoq&itm_medium=popular_widget&itm_campaign=popular_content_list&itm_content=

A complex ksqlDb scenario that includes multiple streams, tables ksql queries (ksql queries documented in the README.md file.
- Kafka-KafkaFlow-sampledata-producer (Sampledata producers written in KafkaFlow)
- Kafka-KsqlDb-KafkaFlow-complex (read the README.md file for the ksqlDb queries)




Integration of .NET for Apache® Spark™ (The .NET for Apache Spark project is part of the .NET Foundation.)

Must read:

- https://dotnet.microsoft.com/en-us/apps/data/spark
- https://learn.microsoft.com/da-dk/previous-versions/dotnet/spark/what-is-spark?WT.mc_id=dotnet-35129-website
- https://learn.microsoft.com/da-dk/previous-versions/dotnet/spark/what-is-apache-spark-dotnet

Requires Java and Spark-util, follow the instructions below:

- https://learn.microsoft.com/en-us/previous-versions/dotnet/spark/tutorials/get-started?source=recommendations&tabs=linux
- https://dotnet.microsoft.com/en-us/apps/data/spark
- https://spark.apache.org/		

Use docker to run the spark container

	- docker run -it --rm spark /opt/spark/bin/spark-sql
 

