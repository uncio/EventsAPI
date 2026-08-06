namespace RU.Uncio.Contracts
{
    public record BookingConfirmed
    {
        public Guid BookingId { get; init; }
        public Guid EventId { get; init; }
        public Guid UserId { get; init; }
        public int SeatsToBook { get; init; }
        public DateTime ProcessedAt { get; init; }
    }
}
