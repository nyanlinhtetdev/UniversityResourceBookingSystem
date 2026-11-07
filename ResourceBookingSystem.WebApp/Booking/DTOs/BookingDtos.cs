using ResourceBookingSystem.WebApp.Booking.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.WebApp.Booking.DTOs
{
    public class BookingRequestDto
    {
        public Guid ResourceId { get; set; }

        public DateTime StartDatetime { get; set; }

        public DateTime EndDatetime { get; set; }
    }

    public class BookingUpdateRequestDto
    {
        public DateTime StartDatetime { get; set; }

        public DateTime EndDatetime { get; set; }
    }

    public class BookingResponseDto
    {
        public Guid BookingId { get; set; }

        public Guid UserId { get; set; }

        public Guid ResourceId { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedDatetime { get; set; }

        public DateTime? ModifiedDatetime { get; set; }

        public DateTime StartDatetime { get; set; }

        public DateTime EndDatetime { get; set; }
    }

    public sealed record BookingResult(
        BookingStatus Status,
        BookingResponseDto? Booking = null);

}
