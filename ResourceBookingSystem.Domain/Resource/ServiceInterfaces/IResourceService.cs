using ResourceBookingSystem.Domain.Resource.DTOs;

namespace ResourceBookingSystem.Domain.Resource.ServiceInterfaces;

public interface IResourceService
{
    Task<IReadOnlyList<ResourceResponseDto>> GetAllAsync(bool includeInactive);

    Task<ResourceResponseDto?> GetByIdAsync(
        Guid resourceId,
        bool includeInactive);

    Task<ResourceResponseDto?> CreateAsync(ResourceRequestDto request);

    Task<bool> CategoryExistsAsync(Guid categoryId);

    Task<ResourceResponseDto?> UpdateAsync(
        Guid resourceId,
        ResourceRequestDto request);

    Task<ResourceResponseDto?> UpdateStatusAsync(
        Guid resourceId,
        string status);
}