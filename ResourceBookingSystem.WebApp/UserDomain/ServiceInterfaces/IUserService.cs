using ResourceBookingSystem.WebApp.UserDomain.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.WebApp.UserDomain.ServiceInterfaces
{
    public interface IUserService
    {
        Task<bool> UpdateProfile(
            Guid userId,
            UserUpdateProfileDto request);
    }
}
