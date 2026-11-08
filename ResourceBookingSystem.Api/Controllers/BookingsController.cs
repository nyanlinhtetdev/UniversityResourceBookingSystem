using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceBookingSystem.WebApp.Booking.DTOs;
using ResourceBookingSystem.WebApp.Booking.Enums;
using ResourceBookingSystem.WebApp.Booking.ServiceInterfaces;
using System.Security.Claims;

namespace ResourceBookingSystem.API.Controllers;

[Route("api/bookings")]
[ApiController]
[Authorize]
public class BookingsController(IBookingService bookingService)
: ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateBooking(
        BookingRequestDto request)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await bookingService.CreateBookingAsync(
            currentUserId,
            request);

        return HandleBookingResult(result, isCreate: true);
    }

    [HttpPut("{bookingId:guid}")]
    public async Task<IActionResult> UpdateBooking(
        Guid bookingId,
        BookingUpdateRequestDto request)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await bookingService.UpdateBookingAsync(
            bookingId,
            currentUserId,
            request);

        return HandleBookingResult(result, isCreate: false);
    }

    [HttpPatch("{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        if(!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var cancelled = await bookingService.CancelBookingAsync(bookingId, currentUserId);

        return cancelled
            ? NoContent()
            : NotFound();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdClaim, out userId);
    }

    private IActionResult HandleBookingResult(
        BookingResult result,
        bool isCreate)
    {
        return result.Status switch
        {
            BookingStatus.Success =>
                CreateSuccessResponse(result, isCreate),

            BookingStatus.InvalidTimeRange =>
                Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid booking time",
                    detail: "The booking must be exactly one hour and use full-hour boundaries."),

            BookingStatus.ResourceUnavailable =>
                Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Resource unavailable",
                    detail: "The requested resource does not exist or is inactive."),

            BookingStatus.SlotAlreadyBooked =>
                Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Time slot unavailable",
                    detail: "The selected time slot has already been booked."),

            BookingStatus.BookingNotFound =>
                Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Booking not found",
                    detail: "The booking does not exist or does not belong to the current user."),

            _ => Problem()
        };
    }

    private IActionResult CreateSuccessResponse(
        BookingResult result,
        bool isCreate)
    {
        if (result.Booking is null)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Booking result is invalid",
                detail: "The booking was processed but no booking data was returned.");
        }

        if (!isCreate)
        {
            return Ok(result.Booking);
        }

        return Created(
            $"/api/bookings/{result.Booking.BookingId}",
            result.Booking);
    }
}