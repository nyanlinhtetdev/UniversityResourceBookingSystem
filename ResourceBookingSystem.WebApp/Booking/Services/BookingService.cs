using Microsoft.EntityFrameworkCore;
using ResourceBookingSystem.Database.AppDbContextModels;
using ResourceBookingSystem.WebApp.Booking.DTOs;
using ResourceBookingSystem.WebApp.Booking.Enums;
using ResourceBookingSystem.WebApp.Booking.ServiceInterfaces;
using ResourceBookingSystem.WebApp.Resource.DTOs;
using ResourceBookingSystem.WebApp.Shared.DTOs;
using BookingEntity = ResourceBookingSystem.Database.AppDbContextModels.Booking;

namespace ResourceBookingSystem.WebApp.Booking.Services
{
    public class BookingService(AppDbContext dbContext): IBookingService
    {
        public async Task<BookingResult> CreateBookingAsync(
            Guid userId,
            BookingRequestDto request)
        {
            if (!BookingDatetimeIsAvailable(
                    request.StartDatetime,
                    request.EndDatetime))
            {
                return new(BookingStatus.InvalidTimeRange);
            }

            if (!await ResourceIsAvailable(request.ResourceId))
            {
                return new(BookingStatus.ResourceUnavailable);
            }

            if (await BookingExistsAsync(
                    request.ResourceId,
                    request.StartDatetime,
                    request.EndDatetime))
            {
                return new(BookingStatus.SlotAlreadyBooked);
            }

            var newBooking = new BookingEntity
            {
                UserId = userId,
                ResourceId = request.ResourceId,
                Status = "Confirmed",
                StartDatetime = request.StartDatetime,
                EndDatetime = request.EndDatetime
            };

            await dbContext.Bookings.AddAsync(newBooking);
            await dbContext.SaveChangesAsync();

            return new(
                BookingStatus.Success,
                RespondBookingData(newBooking));
        }

        public async Task<BookingResult> UpdateBookingAsync(
            Guid bookingId,
            Guid userId,
            BookingUpdateRequestDto request)
        {
            var booking = await dbContext.Bookings
                .FirstOrDefaultAsync(existingBooking =>
                    existingBooking.BookingId == bookingId &&
                    existingBooking.UserId == userId);

            if (booking is null)
            {
                return new(BookingStatus.BookingNotFound);
            }

            if (!BookingDatetimeIsAvailable(
                    request.StartDatetime,
                    request.EndDatetime))
            {
                return new(BookingStatus.InvalidTimeRange);
            }

            if (!await ResourceIsAvailable(booking.ResourceId))
            {
                return new(BookingStatus.ResourceUnavailable);
            }

            if (await BookingExistsAsync(
                    booking.ResourceId,
                    request.StartDatetime,
                    request.EndDatetime,
                    booking.BookingId))
            {
                return new(BookingStatus.SlotAlreadyBooked);
            }

            booking.StartDatetime = request.StartDatetime;
            booking.EndDatetime = request.EndDatetime;
            booking.ModifiedDatetime = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return new(
                BookingStatus.Success,
                RespondBookingData(booking));
        }
        
        public async Task<bool> CancelBookingAsync(
            Guid bookingId,
            Guid userId)
        {
            var booking = await dbContext.Bookings
                .FirstOrDefaultAsync(booking =>
                    booking.BookingId == bookingId &&
                    booking.UserId == userId &&
                    booking.Status != "Cancelled" &&
                    booking.StartDatetime > DateTime.UtcNow); 

            if(booking is null)
            {
                return false;
            }

            booking.Status = "Cancelled";
            booking.ModifiedDatetime = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            return true;
        }

        private static bool BookingDatetimeIsAvailable(
            DateTime startDatetime, 
            DateTime endDatetime)
        {
            if (startDatetime >= endDatetime)
            {
                return false;
            }

            var duration = endDatetime - startDatetime;

            if (duration != TimeSpan.FromHours(1))
            {
                return false;
            }

            return startDatetime.Minute == 0 &&
                   startDatetime.Second == 0 &&
                   startDatetime.Millisecond == 0 &&
                   endDatetime.Minute == 0 &&
                   endDatetime.Second == 0 &&
                   endDatetime.Millisecond == 0;
        }

        private Task<bool> BookingExistsAsync(
            Guid resourceId,
            DateTime startDatetime,
            DateTime endDatetime,
            Guid? excludedBookingId = null)
        {
            return dbContext.Bookings.AnyAsync(booking =>
                booking.ResourceId == resourceId &&
                booking.Status != "Cancelled" &&
                (excludedBookingId == null ||
                 booking.BookingId != excludedBookingId) &&
                booking.StartDatetime < endDatetime &&
                booking.EndDatetime > startDatetime);
        }

        private Task<bool> ResourceIsAvailable(Guid resourceId)
        {
            return dbContext.Resources.AnyAsync(resource => resource.ResourceId == resourceId && resource.Status == "Active");
        }
        private BookingResponseDto RespondBookingData(BookingEntity bookingEntity)
        {
            return new BookingResponseDto
            {
                BookingId = bookingEntity.BookingId,
                UserId = bookingEntity.UserId,
                ResourceId = bookingEntity.ResourceId,
                Status = bookingEntity.Status,
                CreatedDatetime = bookingEntity.CreatedDatetime,
                ModifiedDatetime = bookingEntity.ModifiedDatetime,
                StartDatetime = bookingEntity.StartDatetime,
                EndDatetime = bookingEntity.EndDatetime
            };
        }

        private enum BookingPeriod
        {
            All,
            Upcoming,
            History
        }

        public Task<PaginatedResponse<BookingResponseDto>> GetMyBookingsAsync(
            Guid userId,
            int pageNumber,
            int pageSize)
        {
            return GetBookingsAsync(
                userId,
                pageNumber,
                pageSize,
                BookingPeriod.All);
        }

        public Task<PaginatedResponse<BookingResponseDto>> GetUpcomingBookingsAsync(
            Guid userId,
            int pageNumber,
            int pageSize)
        {
            return GetBookingsAsync(
                userId,
                pageNumber,
                pageSize,
                BookingPeriod.Upcoming);
        }

        public Task<PaginatedResponse<BookingResponseDto>> GetBookingHistoryAsync(
            Guid userId,
            int pageNumber,
            int pageSize)
        {
            return GetBookingsAsync(
                userId,
                pageNumber,
                pageSize,
                BookingPeriod.History);
        }

        public Task<PaginatedResponse<BookingResponseDto>> GetAllBookingsAsync(
            int pageNumber,
            int pageSize)
        {
            return GetBookingsAsync(
                null,
                pageNumber,
                pageSize,
                BookingPeriod.All,
                includeCancelled: true);
        }

        private async Task<PaginatedResponse<BookingResponseDto>> GetBookingsAsync(
            Guid? userId,
            int pageNumber,
            int pageSize,
            BookingPeriod period,
            bool includeCancelled = false)
        {
            var now = DateTime.UtcNow;

            var query = dbContext.Bookings.AsNoTracking().AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(booking => booking.UserId == userId.Value);
            }

            if (!includeCancelled)
            {
                query = query.Where(booking => booking.Status != "Cancelled");
            }

            query = period switch
            {
                BookingPeriod.Upcoming =>
                    query.Where(booking => booking.EndDatetime > now),

                BookingPeriod.History =>
                    query.Where(booking => booking.EndDatetime <= now),

                _ => query
            };

            var totalRecords = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(booking => booking.StartDatetime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(booking => new BookingResponseDto
                {
                    BookingId = booking.BookingId,
                    UserId = booking.UserId,
                    ResourceId = booking.ResourceId,
                    Status = booking.Status,
                    CreatedDatetime = booking.CreatedDatetime,
                    ModifiedDatetime = booking.ModifiedDatetime,
                    StartDatetime = booking.StartDatetime,
                    EndDatetime = booking.EndDatetime
                })
                .ToListAsync();

            return new PaginatedResponse<BookingResponseDto>
            {
                Items = bookings,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(
                    (double)totalRecords / pageSize)
            };
        }

        public async Task<BookingResponseDto?> GetByIdForAdminAsync(
            Guid bookingId)
        {
            var booking = await dbContext.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.BookingId == bookingId);

            return booking is null
                ? null
                : RespondBookingData(booking);
        }

        public async Task<bool> CancelBookingByAdminAsync(Guid bookingId)
        {
            var booking = await dbContext.Bookings
                .FirstOrDefaultAsync(item =>
                    item.BookingId == bookingId &&
                    item.Status != "Cancelled");

            if (booking is null)
            {
                return false;
            }

            booking.Status = "Cancelled";
            booking.ModifiedDatetime = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<BookingResponseDto?> GetByIdAsync(
            Guid bookingId,
            Guid userId)
        {
            var booking = await dbContext.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(booking =>
                    booking.BookingId == bookingId &&
                    booking.UserId == userId);

            return booking is null
                ? null
                : RespondBookingData(booking);
        }
    }
}
