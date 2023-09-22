# Start

Run

docker compose -f docker-compose-kafkaFlow.yml up -d


Control Center
http://localhost:9021/clusters


ksqlDb
SELECT * FROM purchases EMIT CHANGES

