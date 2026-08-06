using Confluent.Kafka;
using RU.Uncio.BookingService.Application.Interfaces;
using RU.Uncio.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RU.Uncio.BookingService.Infrastructure
{
    public class BookingProducer : IBookingProducer
    {
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
                BootstrapServers = "localhost:9092",
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
