using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.Domain.UserDomain.DTOs
{
    public class UserUpdateProfileDto
    {
        public string? FullName { get; set; }

        public string? ContactNumber { get; set; }
    }
}
