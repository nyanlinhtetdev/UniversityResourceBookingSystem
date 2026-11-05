using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceBookingSystem.Domain.Resource.ServiceInterfaces;

namespace ResourceBookingSystem.API.Controllers
{
    [Route("api/resources")]
    [ApiController]
    [Authorize(Roles = "Student,Admin,SuperAdmin")]
    public class ResourcesController(IResourceService resourceService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetResources()
        {
            var resources = await resourceService.GetAllAsync(CanManageResources());

            return Ok(resources);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetResource(Guid id)
        {
            var resource = await resourceService.GetByIdAsync(
                id,
                CanManageResources());

            return resource is null ? NotFound() : Ok(resource);
        }

        private bool CanManageResources()
        {
            return User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        }
    }
}
