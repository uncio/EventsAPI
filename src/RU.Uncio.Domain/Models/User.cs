using System;
using System.Collections.Generic;
using System.Text;

namespace RU.Uncio.Domain.Models
{
    /// <summary>
    /// User model
    /// </summary>
    public class User
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// USer ID
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// User login name
        /// </summary>
        public string Login { get; set; } = null!;
        /// <summary>
        /// User hashed password
        /// </summary>
        public string HashedPassword { get; set; } = null!;
        /// <summary>
        /// User role
        /// </summary>
        public Roles Role { get; set; }
        /// <summary>
        /// User bookings
        /// </summary>
        public List<Booking> Bookings { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        public User() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        public User(string login, string password, Roles role = Roles.User)
        {
            Id = Guid.NewGuid();
            Login = login;
            HashedPassword = password;
            Role = role;
        }

        /// <summary>
        /// Tries to add a booking for a user if booking limit is not exceeded
        /// </summary>
        /// <param name="booking"></param>
        /// <returns></returns>
        public bool TryAddBooking(Booking booking)
        {
            if (Bookings.Count < 10)
            {
                Bookings.Add(booking);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// removes booking on cancellation
        /// </summary>
        /// <param name="bookingId"></param>
        public void RemoveBooking(Guid bookingId)
        {
            var bookingToRemove = Bookings.FirstOrDefault(b => b.Id.Equals(bookingId));
            if (bookingToRemove != null)
            {
                Bookings.Remove(bookingToRemove);
            }
        }
    }
}
