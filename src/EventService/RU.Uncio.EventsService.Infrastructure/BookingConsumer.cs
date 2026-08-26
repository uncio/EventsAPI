using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
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
    /// <summary>
    /// background service to consume booking confirmation
    /// </summary>
    /// <param name="scFactory"></param>
    /// <param name="log"></param>
    /// <param name="config"></param>
    public class BookingConsumer(IServiceScopeFactory scFactory,
                                 ILogger<BookingConsumer> log,
                                 IConfiguration config) : BackgroundService
    {

        private readonly ILogger<BookingConsumer> logger = log;
        private readonly IServiceScopeFactory scopeFactory = scFactory;
        private readonly IConfiguration configuration = config;
        private static readonly SemaphoreSlim processingSemaphore = new(1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Task.Run нужен, чтобы Consume (блокирующий вызов) не блокировал хост при старте
            return Task.Run(() => Consume(stoppingToken), stoppingToken);
        }

        internal async Task Consume(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "booking-processing",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(Constants.TOPIC);

            logger.LogInformation("Consumer запущен. Ожидание сообщений из топика 'booking-confirmed'...");

            await processingSemaphore.WaitAsync(stoppingToken);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    var bookingRequest = JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value);
                    using var scope = scopeFactory.CreateScope();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventsService>();
                    var targetEvent = await eventService.GetEventAsync(bookingRequest!.EventId, stoppingToken);
                    
                    if(targetEvent != null)
                    {
                        var bookingResult = targetEvent.TryReserveSeats(bookingRequest.SeatsToBook);                        

                        if (!bookingResult)
                        {
                            logger.LogError("No available seats for event {EventId}", bookingRequest.EventId);
                        }
                        else
                        {
                            await eventService.UpdateEventAsync(targetEvent.Id, targetEvent, stoppingToken);

                            logger.LogInformation(
                            "Booking recieved [{Offset}] BookingId={BookingId}, EventId={EventId}, UserId={UserId}," +
                            " SeatsToBook={SeatsToBook}, ProcessedAt={ProcessedAt}",
                            consumeResult.TopicPartitionOffset,
                            bookingRequest?.BookingId,
                            bookingRequest?.EventId,
                            bookingRequest?.UserId,
                            bookingRequest?.SeatsToBook,
                            bookingRequest?.ProcessedAt);
                        }
                    }
                    else
                    {
                        logger.LogError("Event with ID {EventId} is not found in the collection", bookingRequest.EventId);
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
                processingSemaphore.Release();            
                consumer.Close();
            }
        }
    }
}
