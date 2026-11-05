using ResourceBookingSystem.Domain.UserDomain.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.Domain.UserDomain.ServiceInterfaces
{
    public interface IUserService
    {
        Task<bool> UpdateProfile(
            Guid userId,
            UserUpdateProfileDto request);
    }
}
