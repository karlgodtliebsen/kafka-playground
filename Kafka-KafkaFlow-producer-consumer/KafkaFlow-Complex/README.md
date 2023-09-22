# Start

Run
````
docker compose -f docker-compose-confluent.yml up -d
````

Control Center
http://localhost:9021/clusters


````
CREATE TABLE TopicIdentityTables (
    ID VARCHAR PRIMARY KEY,
    ID1 VARCHAR,
    ID2 VARCHAR,
    ID3 VARCHAR,
    ID4 VARCHAR
) WITH (
    KAFKA_TOPIC='topicidentitymap',
    VALUE_FORMAT='JSON'
);
````'

````
CREATE TABLE QUERYABLE_TOPICIDENTITYTABLES AS SELECT * FROM TOPICIDENTITYTABLES;
````

````
SELECT * FROM TopicIdentityTables EMIT CHANGES;
````
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
````
SELECT * FROM DATASOURCESTREAM EMIT CHANGES;
````
````
CREATE STREAM EnrichedDataSourceStream AS
SELECT 
    s.Id,
    s.Name,
    s.Prop1,
    s.Prop2,
    s.Prop3,
    s.Prop4,
    t.Id1,
    t.Id2,
    t.Id3,
    t.Id4
FROM DataSourceStream s
LEFT JOIN TopicIdentityTable t
ON s.Id = t.ID;
````
````
SELECT * FROM EnrichedDataSourceStream EMIT CHANGES;
````


## references

- https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/ksqlDb.RestApi.Client/KSql/RestApi/Statements/StatementTemplates.cs#L20
- https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/docs/statements.md
- https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/docs/streams_and_tables.md

