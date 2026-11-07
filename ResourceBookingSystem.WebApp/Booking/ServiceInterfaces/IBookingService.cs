using ResourceBookingSystem.WebApp.Booking.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.WebApp.Booking.ServiceInterfaces
{
    public interface IBookingService
    {
        Task<BookingResult> CreateBookingAsync(Guid UserId, BookingRequestDto request);
        Task<BookingResult> UpdateBookingAsync(Guid bookingId, Guid userId, BookingUpdateRequestDto request);
    }
}
