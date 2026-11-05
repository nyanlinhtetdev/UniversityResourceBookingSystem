using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResourceBookingSystem.Domain.UserDomain.Services;
using ResourceBookingSystem.Domain.UserDomain.DTOs;
using ResourceBookingSystem.Domain.UserDomain.ServiceInterfaces;
using System.Security.Claims;

namespace ResourceBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService userService) : ControllerBase
    {
        [Authorize]
        [HttpPatch("me")]
        public async Task<IActionResult> UpdateProfile(UserUpdateProfileDto request)
        {
            var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized();
            }

            var updated = await userService.UpdateProfile(currentUserId, request);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
