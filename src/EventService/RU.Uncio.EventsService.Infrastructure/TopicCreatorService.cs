using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RU.Uncio.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RU.Uncio.EventsService.Infrastructure
{
    /// <summary>
    /// topic creator
    /// </summary>
    /// <param name="config"></param>
    /// <param name="log"></param>
    public class TopicCreatorService(IConfiguration config, ILogger<TopicCreatorService> log) : IHostedService
    {
        private readonly IConfiguration configuration = config;
        private readonly ILogger<TopicCreatorService> logger = log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            CreateTopicIfNotExists(Constants.TOPIC, 1, 1);

            return Task.CompletedTask;
        }

        private void CreateTopicIfNotExists(string topicName, int v1, int v2)
        {
            var config = new AdminClientConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] };
            using var adminClient = new AdminClientBuilder(config).Build();

            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            var topicsMetadata = metadata.Topics;
            var topicNames = metadata.Topics.Select(a => a.Topic).ToList();

            var topicExists = topicNames.Any(t => t.Equals(Constants.TOPIC));

            if (!topicExists)
            {             
                try
                {
                    adminClient.CreateTopicsAsync(new List<TopicSpecification>{ new TopicSpecification
                    {
                        Name = topicName,
                        ReplicationFactor = 1,
                        NumPartitions = 1
                    } });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Topic creation failed");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
