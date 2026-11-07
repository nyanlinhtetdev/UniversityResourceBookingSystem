using Microsoft.EntityFrameworkCore;
using ResourceBookingSystem.Database.AppDbContextModels;
using ResourceBookingSystem.WebApp.Booking.DTOs;
using ResourceBookingSystem.WebApp.Booking.Enums;
using ResourceBookingSystem.WebApp.Booking.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using BookingEntity = ResourceBookingSystem.Database.AppDbContextModels.Booking;

namespace ResourceBookingSystem.WebApp.Booking.Services
{
    public class BookingService(AppDbContext dbContext): IBookingService
    {
        public async Task<BookingResult> CreateBookingAsync(Guid UserId, BookingRequestDto request)
        {
            if (!BookingDatetimeIsAvailable(request))
            {
                return new(BookingStatus.InvalidTimeRange);
            }

            if(!await ResourceIsAvailable(request.ResourceId))
            {
                return new(BookingStatus.ResourceUnavailable);
            }

            if (await BookingExistsAsync(request))
            {
                return new(BookingStatus.SlotAlreadyBooked);
            }

            BookingEntity newBooking = new BookingEntity
            {
                UserId = UserId,
                ResourceId = request.ResourceId,
                Status = "Confirmed",
                StartDatetime = request.StartDatetime,
                EndDatetime = request.EndDatetime
            };
            await dbContext.Bookings.AddAsync(newBooking);
            await dbContext.SaveChangesAsync();
            return new(
                BookingStatus.Success, 
                RespondBookingData(newBooking)
            );
        }

        private bool BookingDatetimeIsAvailable(BookingRequestDto request)
        {
            if (request.StartDatetime >= request.EndDatetime)
            {
                return false;
            }

            var duration = request.EndDatetime - request.StartDatetime;

            if (duration != TimeSpan.FromHours(1))
            {
                return false;
            }

            if (request.StartDatetime.Minute != 0 ||
                request.StartDatetime.Second != 0 ||
                request.StartDatetime.Millisecond != 0 ||
                request.EndDatetime.Minute != 0 ||
                request.EndDatetime.Second != 0 ||
                request.EndDatetime.Millisecond != 0)
            {
                return false;
            }
            return true;
        }

        private Task<bool> BookingExistsAsync(BookingRequestDto request)
        {
            return dbContext.Bookings.AnyAsync(
                booking => booking.ResourceId == request.ResourceId &&
                           booking.Status != "Cancelled" &&
                           booking.StartDatetime == request.StartDatetime &&
                           booking.EndDatetime == request.EndDatetime);
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
    }
}
