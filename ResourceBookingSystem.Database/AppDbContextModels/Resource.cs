using System;
using System.Collections.Generic;

namespace ResourceBookingSystem.Database.AppDbContextModels;

public partial class Resource
{
    public Guid ResourceId { get; set; }

    public string Name { get; set; } = null!;

    public string Location { get; set; } = null!;

    public int? Capacity { get; set; }

    public string Status { get; set; } = null!;

    public Guid ResourceCategoryId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ResourceCategory ResourceCategory { get; set; } = null!;
}
