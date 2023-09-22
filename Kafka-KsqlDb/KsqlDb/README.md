# Start

Run

docker compose -f docker-compose-confluent.yml up -d


Control Center
http://localhost:9021/clusters


ksqlDb
SELECT * FROM tweets EMIT CHANGES;

SELECT Id, LATEST_BY_OFFSET(MESSAGE, False) Earliest FROM Tweets GROUP BY Id EMIT CHANGES LIMIT 2;




## references

- https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/ksqlDb.RestApi.Client/KSql/RestApi/Statements/StatementTemplates.cs#L20
- https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/docs/statements.md
- https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/docs/streams_and_tables.md


