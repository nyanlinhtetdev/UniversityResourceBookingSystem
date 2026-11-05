using System;
using System.Collections.Generic;

namespace ResourceBookingSystem.Database.AppDbContextModels;

public partial class RefreshToken
{
    public Guid RefreshTokenId { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiredDatetime { get; set; }

    public DateTime CreatedDatetime { get; set; }

    public DateTime? RevokedDatetime { get; set; }

    public virtual User User { get; set; } = null!;
}
