using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResourceBookingSystem.WebApp.Booking.DTOs;
using ResourceBookingSystem.WebApp.Booking.Enums;
using ResourceBookingSystem.WebApp.Booking.ServiceInterfaces;
using System.Security.Claims;

namespace ResourceBookingSystem.API.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingsController(IBookingService bookingService): ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBooking(BookingRequestDto request)
        {
            var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized();
            }
            var result = await bookingService.CreateBookingAsync(currentUserId, request);

            return result.Status switch
            {
                BookingStatus.Success =>
                   Created(
                       $"/api/bookings/{result.Booking!.BookingId}",
                       result.Booking),

                BookingStatus.InvalidTimeRange =>
                   Problem(
                       statusCode: StatusCodes.Status400BadRequest,
                       title: "Invalid booking time",
                       detail: "The booking must be exactly one hour and use full-hour boundaries."),

                BookingStatus.ResourceUnavailable =>
                   Problem(
                       statusCode: StatusCodes.Status404NotFound,
                       title: "Resource not found",
                       detail: "The requested resource does not exist."),

                BookingStatus.SlotAlreadyBooked =>
                   Problem(
                       statusCode: StatusCodes.Status409Conflict,
                       title: "Time slot unavailable",
                       detail: "The selected time slot has already been booked."),

                _ => Problem()
            };
        }
    }
}
