using ResourceBookingSystem.WebApp.Booking.DTOs;
using ResourceBookingSystem.WebApp.Shared.DTOs;

namespace ResourceBookingSystem.WebApp.Booking.ServiceInterfaces;

public interface IBookingService
{
    Task<PaginatedResponse<BookingResponseDto>> GetMyBookingsAsync(
        Guid userId,
        int pageNumber,
        int pageSize);

    Task<PaginatedResponse<BookingResponseDto>> GetUpcomingBookingsAsync(
        Guid userId,
        int pageNumber,
        int pageSize);

    Task<PaginatedResponse<BookingResponseDto>> GetBookingHistoryAsync(
        Guid userId,
        int pageNumber,
        int pageSize);

    Task<BookingResponseDto?> GetByIdAsync(
        Guid bookingId,
        Guid userId);

    Task<BookingResult> CreateBookingAsync(
        Guid userId,
        BookingRequestDto request);

    Task<BookingResult> UpdateBookingAsync(
        Guid bookingId,
        Guid userId,
        BookingUpdateRequestDto request);

    Task<bool> CancelBookingAsync(
        Guid bookingId,
        Guid userId);
}
