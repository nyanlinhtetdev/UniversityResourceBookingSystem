using System.ComponentModel.DataAnnotations;

namespace ResourceBookingSystem.WebApp.ResourceCategory.DTOs;

public sealed class ResourceCategoryRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
}

public sealed class ResourceCategoryResponseDto
{
    public Guid ResourceCategoryId { get; init; }
    public string Name { get; init; } = null!;
}

public enum ResourceCategoryMutationStatus
{
    Success,
    NotFound,
    NameAlreadyExists,
    HasResources
}

public sealed record ResourceCategoryResult(
    ResourceCategoryMutationStatus Status,
    ResourceCategoryResponseDto? Category = null);