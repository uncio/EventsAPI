using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RU.Uncio.BookingService.Application.Backservices;
using RU.Uncio.BookingService.Application.Interfaces;
using RU.Uncio.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RU.Uncio.BookingService.Infrastructure
{
    public class BookingProducer(IConfiguration config) : IBookingProducer, IDisposable
    {
        private readonly IConfiguration configuration = config;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public async Task PublishBooking(Guid bookingId, Guid eventId, Guid userId, int seatsToBook = 1)
        {
            var orderCreated = new BookingConfirmed
            {
                BookingId = bookingId,
                EventId = eventId,
                UserId = userId,
                SeatsToBook = seatsToBook,
                ProcessedAt = DateTime.Now.ToUniversalTime()
            };

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                Acks = Acks.All
            };

            using var producer = new ProducerBuilder<string, string>(config).Build();
            var result = await producer.ProduceAsync(Constants.TOPIC, new Message<string, string>
            {
                Key = eventId.ToString(),
                Value = JsonSerializer.Serialize(orderCreated)
            });
        }
    }
}
