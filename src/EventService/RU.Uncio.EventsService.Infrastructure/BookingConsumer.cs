using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RU.Uncio.Contracts;
using RU.Uncio.EventService.Application.Interfaces;
using RU.Uncio.EventService.Domain.Exceptions;
using RU.Uncio.EventService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RU.Uncio.EventsService.Infrastructure
{
    public class BookingConsumer(IServiceScopeFactory scFactory, ILogger<BookingConsumer> log) : BackgroundService
    {

        private readonly ILogger<BookingConsumer> logger = log;
        private readonly IServiceScopeFactory scopeFactory = scFactory;
        private static readonly SemaphoreSlim processingSemaphore = new(1, 1);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Task.Run нужен, чтобы Consume (блокирующий вызов) не блокировал хост при старте
            return Task.Run(() => Consume(stoppingToken), stoppingToken);
        }

        internal async Task Consume(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "order-processing",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe("orders");

            logger.LogInformation("Consumer запущен. Ожидание сообщений из топика 'orders'...");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    var order = JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value);
                    using var scope = scopeFactory.CreateScope();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventsService>();
                    var targetEvent = await eventService.GetEventAsync(order.EventId, stoppingToken);
                    
                    if(targetEvent != null)
                    {
                        var bookingResult = targetEvent.TryReserveSeats(order.SeatsToBook);

                        if (!bookingResult)
                        {
                            logger.LogError($"No available seats for event {order.EventId}");
                        }
                        else
                        {
                            logger.LogInformation(
                            "Booking recieved [{Offset}] BookingId={BookingId}, EventId={EventId}, UserId={UserId}," +
                            " SeatsToBook={SeatsToBook}, ProcessedAt={ProcessedAt}",
                            consumeResult.TopicPartitionOffset,
                            order?.BookingId,
                            order?.EventId,
                            order?.UserId,
                            order?.SeatsToBook,
                            order?.ProcessedAt);
                        }
                    }
                    else
                    {
                        throw new MissingEventException($"Event with ID {order.EventId} is not found in the collection");
                    }

                    consumer.StoreOffset(consumeResult);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Consumer stopped.");
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
