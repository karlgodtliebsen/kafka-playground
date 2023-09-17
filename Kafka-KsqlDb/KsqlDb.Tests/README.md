# Start

Run

docker compose -f docker-compose.yml up -d


Control Center
http://localhost:9021/clusters


ksqlDb
SELECT * FROM tweets EMIT CHANGES;

SELECT Id, LATEST_BY_OFFSET(MESSAGE, False) Earliest FROM Tweets GROUP BY Id EMIT CHANGES LIMIT 2;



