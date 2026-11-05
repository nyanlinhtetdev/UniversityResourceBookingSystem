using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceBookingSystem.Domain.Resource.DTOs;
using ResourceBookingSystem.Domain.Resource.ServiceInterfaces;

namespace ResourceBookingSystem.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController(IResourceService resourceService) : ControllerBase
    {
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
    }
}
