using System;
using System.Collections.Generic;

namespace ResourceBookingSystem.Database.AppDbContextModels;

public partial class Booking
{
    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public Guid ResourceId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDatetime { get; set; }

    public DateTime? ModifiedDatetime { get; set; }

    public DateTime StartDatetime { get; set; }

    public DateTime EndDatetime { get; set; }

    public virtual Resource Resource { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
