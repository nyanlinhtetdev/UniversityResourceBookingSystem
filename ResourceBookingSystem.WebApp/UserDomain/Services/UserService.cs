using Microsoft.EntityFrameworkCore;
using ResourceBookingSystem.Database.AppDbContextModels;
using ResourceBookingSystem.WebApp.UserDomain.DTOs;
using ResourceBookingSystem.WebApp.UserDomain.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.WebApp.UserDomain.Services
{
    public class UserService(AppDbContext dbContext): IUserService
    {
        public async Task<bool> UpdateProfile(
            Guid userId,
            UserUpdateProfileDto request)
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user is null)
            {
                return false;
            }

            var hasChanges = false;

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.ContactNumber))
            {
                user.ContactNumber = request.ContactNumber.Trim();
                hasChanges = true;
            }

            if (!hasChanges)
            {
                return false;
            }

            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}
