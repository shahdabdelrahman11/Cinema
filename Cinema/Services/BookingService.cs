using Cinema.Models;
using Cinema.Repositories;
using System;
using System.Collections.Generic;

namespace Cinema.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _bookings;
        private readonly IRepository<Movie> _movies;

        public BookingService(IRepository<Booking> bookings, IRepository<Movie> movies)
        {
            _bookings = bookings;
            _movies = movies;
        }

        public IEnumerable<Booking> GetAll()
        {
            return _bookings
                .GetAll(b => b.Movie!)
                .OrderByDescending(b => b.BookingDate)
                .ToList();
        }

        public ServiceResult Create(Booking booking)
        {
            var result = new ServiceResult();

            var movie = _movies.GetById(booking.MovieId);
            if (movie == null || !movie.Status)
            {
                result.AddError(string.Empty, "This movie is not available for booking.");
                return result;
            }

            if (booking.NumberOfTickets < 1 || booking.NumberOfTickets > 10)
                result.AddError(nameof(Booking.NumberOfTickets), "Choose between 1 and 10 tickets.");

            if (booking.ShowDateTime < DateTime.Now)
                result.AddError(nameof(Booking.ShowDateTime), "Please choose a future show date and time.");

            if (!result.Succeeded)
                return result;

            try
            {
                booking.Id = 0;
                booking.BookingDate = DateTime.Now;
                booking.PaymentMethod = "Pay at Cinema";

                _bookings.Add(booking);
                _bookings.Save();
            }
            catch (Exception ex)
            {
                var error = ex.InnerException?.Message ?? ex.Message;
                result.AddError(string.Empty, "Database Error: " + error);
            }

            return result;
        }

        public bool Delete(int id)
        {
            if (_bookings.GetById(id) == null)
                return false;

            _bookings.Delete(id);
            _bookings.Save();
            return true;
        }
    }
}