using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceBookingSystem.WebApp.Booking.DTOs;
using ResourceBookingSystem.WebApp.Booking.Enums;
using ResourceBookingSystem.WebApp.Booking.ServiceInterfaces;
using ResourceBookingSystem.WebApp.Shared.DTOs;
using System.Security.Claims;

namespace ResourceBookingSystem.API.Controllers;

[Route("api/bookings")]
[ApiController]
[Authorize(Roles = "Student,Admin,SuperAdmin")]
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

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        return await GetBookingListAsync(
            pageNumber,
            pageSize,
            (userId, page, size) =>
                bookingService.GetMyBookingsAsync(
                    userId,
                    page,
                    size));
    }

    [HttpGet("my/upcoming")]
    public async Task<IActionResult> GetUpcomingBookings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        return await GetBookingListAsync(
            pageNumber,
            pageSize,
            (userId, page, size) =>
                bookingService.GetUpcomingBookingsAsync(
                    userId,
                    page,
                    size));
    }

    [HttpGet("my/history")]
    public async Task<IActionResult> GetBookingHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        return await GetBookingListAsync(
            pageNumber,
            pageSize,
            (userId, page, size) =>
                bookingService.GetBookingHistoryAsync(
                    userId,
                    page,
                    size));
    }

    private async Task<IActionResult> GetBookingListAsync(
        int pageNumber,
        int pageSize,
        Func<Guid, int, int, Task<PaginatedResponse<BookingResponseDto>>> query)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid pagination",
                detail: "Page number must be at least 1 and page size must be between 1 and 100.");
        }

        var result = await query(
            currentUserId,
            pageNumber,
            pageSize);

        return Ok(result);
    }


    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> GetBooking(Guid bookingId)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var booking = await bookingService.GetByIdAsync(
            bookingId,
            currentUserId);

        return booking is null
            ? NotFound()
            : Ok(booking);
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