# How to start:

Run
````
docker compose -f docker-compose-confluent.yml up -d
````

### Start Control Center
- http://localhost:9021/clusters


### Precondition
Create 3 Topics
More in the projects in 'Kafka-KsqlDb-KafkaFlow-complex'

> For testdata
- topicdatasource
- topicidentitymap

> For output data:
- topicoutbound


## Pure ksqlDb approach:
#### Create the Table view on the topicindeitymap topic

````
CREATE TABLE TopicIdentityTables (
    ID VARCHAR PRIMARY KEY,
    ID1 VARCHAR,
    ID2 VARCHAR,
    ID3 VARCHAR,
    ID4 VARCHAR,
    DateTimeOffset TIMESTAMP
) WITH (
    KAFKA_TOPIC='topicidentitymap',
    VALUE_FORMAT='JSON'
);
````

Select statement for testing the table view
````
SELECT * FROM TOPICIDENTITYTABLES EMIT CHANGES;
````

Create the Stream on the topicdatasource topic
````
CREATE STREAM DataSourceStream (
    Id VARCHAR KEY,
    Name VARCHAR,
    Prop1 VARCHAR,
    Prop2 VARCHAR,
    Prop3 VARCHAR,
    Prop4 VARCHAR
) WITH (
    KAFKA_TOPIC='topicdatasource',
    VALUE_FORMAT='JSON'
);
````

Select statement for testing the stream
````
SELECT * FROM DATASOURCESTREAM EMIT CHANGES;
````


An alternative Create statement for the table view

````
CREATE OR REPLACE TABLE TopicIdentityTables (
        Id VARCHAR PRIMARY KEY,
        Id1 VARCHAR,
        Id2 VARCHAR,
        Id3 VARCHAR,
        Id4 VARCHAR,
        DateTimeOffset TIMESTAMP
      ) WITH ( KAFKA_TOPIC='topicidentitymap', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );
````

Test by running the  Select expression joining the datasourcestream and the table view
````
SELECT ds.Id, 
       ds.Name, 
       ds.Prop1, 
       ds.Prop2, 
       ds.Prop3, 
       ds.Prop4,      
       qt.ID1, 
       qt.ID2,
       qt.ID3,
       qt.ID4 
FROM datasourcestream ds 
LEFT JOIN topicidentitytables qt 
ON ds.Id = qt.Id 
EMIT CHANGES;
````


Create the output stream
````
CREATE STREAM OutboundStream
WITH (KAFKA_TOPIC='topicoutbound', VALUE_FORMAT='JSON') AS 
SELECT ds.Id Id, 
       ds.Name, 
       ds.Prop1, 
       ds.Prop2, 
       ds.Prop3, 
       ds.Prop4,      
       qt.ID1, 
       qt.ID2,
       qt.ID3,
       qt.ID4 
FROM datasourcestream ds 
LEFT JOIN topicidentitytables qt 
ON ds.Id = qt.Id;

````


````
SELECT * FROM OutboundStream EMIT CHANGES;
````


## dotnet KafkaFlow and ksqlDb approach
Use the project in the folder 'Kafka-KsqlDb-KafkaFlow-complex'



If using a pullquery you might have to create a queryable table 
````
CREATE TABLE QUERYABLE_TOPICIDENTITYTABLES AS SELECT * FROM TOPICIDENTITYTABLES;
````


## references

- https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/ksqlDb.RestApi.Client/KSql/RestApi/Statements/StatementTemplates.cs#L20
- https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/docs/statements.md
- https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/docs/streams_and_tables.md




## Opensearch Connector
- https://opensearch.org/docs/latest/install-and-configure/install-opensearch/docker/#important-host-settings


### OpenSearch docker:
````
docker pull opensearchproject/opensearch:latest
docker run -p 9200:9200 -p 9600:9600 -e "discovery.type=single-node" --name opensearch-node -d opensearchproject/opensearch:latest
````

````
curl -X GET "https://localhost:9200" -ku admin:admin
curl -X GET "https://localhost:9200/_cat/nodes?v" -ku admin:admin
curl -X GET "https://localhost:9200/_cat/plugins?v" -ku admin:admin
````
````
docker stop opensearch-node
````

````
docker pull opensearchproject/opensearch-dashboards:latest
docker run -p 5601:5601 -e 'OPENSEARCH_HOSTS=["http://opensearch-node:9200"]' -e "DISABLE_SECURITY_DASHBOARDS_PLUGIN=true" --name o opensearch-dashboards -d opensearchproject/opensearch-dashboards:latest
````

#### Dashboard parameters for Docker Desktop:
````
- OPENSEARCH_HOSTS=["http://opensearch-node:9200"]

- DISABLE_SECURITY_DASHBOARDS_PLUGIN=true

262144
````

docker compose:

````
cd .\kafka-playground\docker
docker-compose -f .\docker-compose-opensearch.yml up -d
````


