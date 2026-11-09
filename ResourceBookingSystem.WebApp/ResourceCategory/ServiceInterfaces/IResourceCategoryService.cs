using ResourceBookingSystem.WebApp.ResourceCategory.DTOs;

namespace ResourceBookingSystem.WebApp.ResourceCategory.ServiceInterfaces;

public interface IResourceCategoryService
{
    Task<IReadOnlyList<ResourceCategoryResponseDto>> GetAllAsync();

    Task<ResourceCategoryResult> CreateAsync(
        ResourceCategoryRequestDto request);

    Task<ResourceCategoryResult> UpdateAsync(
        Guid categoryId,
        ResourceCategoryRequestDto request);

    Task<ResourceCategoryResult> DeleteAsync(Guid categoryId);
}