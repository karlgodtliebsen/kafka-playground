
//}"Kafka": {
//  "ProducerSettings": {
//    "BootstrapServers": "localhost:9092",
//    "SaslMechanism": "Plain",
//    "SecurityProtocol": "SaslSsl",
//    "SaslUsername": "<confluent cloud key>",
//    "SaslPassword": "<confluent cloud secret>"
//  },
//  "ConsumerSettings": {
//    "BootstrapServers": "localhost:9092",
//    "GroupId": "web-example-group",
//    "SaslMechanism": "Plain",
//    "SecurityProtocol": "SaslSsl",
//    "SaslUsername": "<confluent cloud key>",
//    "SaslPassword": "<confluent cloud secret>"
//  },
//  "RequestTimeTopic": "request_times",
//  "FrivolousTopic": "frivolous_topic",
//  "BootstrapServers": "localhost:9092"


//"SecurityProtocol": "SaslSsl",
//"SaslMechanism": "plain",
//"SaslUsername": "<api-key>",
//"SaslPassword": "<api-secret>",
//"SslCaLocation": "probe"

//GroupId = "csharp-consumer",
//EnableAutoOffsetStore = false,
//EnableAutoCommit = true,
//StatisticsIntervalMs = 5000,
//SessionTimeoutMs = 6000,
//AutoOffsetReset = AutoOffsetReset.Earliest,
//EnablePartitionEof = true,
//// A good introduction to the CooperativeSticky assignor and incremental rebalancing:
//// https://www.confluent.io/blog/cooperative-rebalancing-in-kafka-streams-consumer-ksqldb/
//PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky
