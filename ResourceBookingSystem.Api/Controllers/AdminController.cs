using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceBookingSystem.WebApp.Resource.DTOs;
using ResourceBookingSystem.WebApp.Resource.ServiceInterfaces;
using ResourceBookingSystem.WebApp.ResourceCategory.DTOs;
using ResourceBookingSystem.WebApp.ResourceCategory.ServiceInterfaces;
using ResourceBookingSystem.WebApp.Booking.ServiceInterfaces;

namespace ResourceBookingSystem.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController(
        IResourceService resourceService,
        IResourceCategoryService resourceCategoryService,
        IBookingService bookingService) : ControllerBase
    {
        [HttpGet("bookings")]
        public async Task<IActionResult> GetAllBookings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!IsValidPagination(pageNumber, pageSize))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid pagination",
                    detail: "Page number must be at least 1 and page size must be between 1 and 100.");
            }

            var bookings = await bookingService.GetAllBookingsAsync(
                pageNumber,
                pageSize);

            return Ok(bookings);
        }

        [HttpGet("bookings/{id:guid}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            var booking = await bookingService.GetByIdForAdminAsync(id);

            return booking is null
                ? NotFound()
                : Ok(booking);
        }

        [HttpPatch("bookings/{id:guid}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var cancelled = await bookingService.CancelBookingByAdminAsync(id);

            return cancelled
                ? NoContent()
                : NotFound();
        }

        [HttpPost("resource-categories")]
        public async Task<IActionResult> CreateResourceCategory(
            ResourceCategoryRequestDto request)
        {
            var result = await resourceCategoryService.CreateAsync(request);

            return result.Status switch
            {
                ResourceCategoryMutationStatus.Success =>
                    Created("/api/resource-categories", result.Category),

                ResourceCategoryMutationStatus.NameAlreadyExists =>
                    Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Category already exists",
                        detail: "A resource category with this name already exists."),

                _ => Problem()
            };
        }

        [HttpPut("resource-categories/{id:guid}")]
        public async Task<IActionResult> UpdateResourceCategory(
            Guid id,
            ResourceCategoryRequestDto request)
        {
            var result = await resourceCategoryService.UpdateAsync(id, request);

            return result.Status switch
            {
                ResourceCategoryMutationStatus.Success =>
                    Ok(result.Category),

                ResourceCategoryMutationStatus.NotFound =>
                    NotFound(),

                ResourceCategoryMutationStatus.NameAlreadyExists =>
                    Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Category already exists",
                        detail: "A resource category with this name already exists."),

                _ => Problem()
            };
        }

        [HttpDelete("resource-categories/{id:guid}")]
        public async Task<IActionResult> DeleteResourceCategory(Guid id)
        {
            var result = await resourceCategoryService.DeleteAsync(id);

            return result.Status switch
            {
                ResourceCategoryMutationStatus.Success =>
                    NoContent(),

                ResourceCategoryMutationStatus.NotFound =>
                    NotFound(),

                ResourceCategoryMutationStatus.HasResources =>
                    Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Category is in use",
                        detail: "A category cannot be deleted while resources use it."),

                _ => Problem()
            };
        }

        [HttpPost("resources")]
        public async Task<IActionResult> CreateResource(
            ResourceRequestDto request)
        {
            if (!await resourceService.CategoryExistsAsync(
                    request.ResourceCategoryId))
            {
                return BadRequest("The resource category does not exist.");
            }

            var resource = await resourceService.CreateAsync(request);

            return Created(
                $"/api/resources/{resource!.ResourceId}",
                resource);
        }

        [HttpPut("resources/{id:guid}")]
        public async Task<IActionResult> UpdateResource(
            Guid id,
            ResourceRequestDto request)
        {
            if (!await resourceService.CategoryExistsAsync(
                    request.ResourceCategoryId))
            {
                return BadRequest("The resource category does not exist.");
            }

            var resource = await resourceService.UpdateAsync(id, request);

            return resource is null ? NotFound() : Ok(resource);
        }

        [HttpPatch("resources/{id:guid}/status")]
        public async Task<IActionResult> UpdateResourceStatus(
            Guid id,
            ResourceStatusUpdateDto request)
        {
            if (!string.Equals(request.Status, "Active", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(request.Status, "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Status must be Active or Inactive.");
            }

            var resource = await resourceService.UpdateStatusAsync(
                id,
                request.Status);

            return resource is null ? NotFound() : Ok(resource);
        }

        private static bool IsValidPagination(
            int pageNumber,
            int pageSize)
        {
            return pageNumber >= 1 &&
                   pageSize >= 1 &&
                   pageSize <= 100;
        }
    }
}
