using System;
using System.Collections.Generic;
using System.Text;

namespace RU.Uncio.BookingService.Application.Interfaces
{
    public interface IBookingProducer
    {
        Task PublishBooking(Guid bookingId, Guid eventId, Guid userId, int seatsToBook = 1);
    }
}
