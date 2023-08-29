﻿using KsqlDb.Configuration;
using KsqlDb.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace KsqlDb.HostServices;

public class KafkaProducerService : BackgroundService
{
    private readonly KafkaProducer producer;
    private readonly KafkaConfiguration settings;

    public KafkaProducerService(KafkaProducer producer, IOptions<KafkaConfiguration> settings)
    {
        this.producer = producer;
        this.settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await producer.Produce(settings.TopicSource);
    }
}