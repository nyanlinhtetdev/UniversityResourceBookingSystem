using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.WebApp.Booking.Enums
{
    public enum BookingStatus
    {
        Success,
        InvalidTimeRange,
        ResourceUnavailable,
        SlotAlreadyBooked
    }
}
