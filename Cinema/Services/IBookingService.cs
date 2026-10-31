using Cinema.Models;
using System.Collections.Generic;

namespace Cinema.Services
{
    public interface IBookingService
    {
        IEnumerable<Booking> GetAll();
        ServiceResult Create(Booking booking);
        bool Delete(int id);
    }
}