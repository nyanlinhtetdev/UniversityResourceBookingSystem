using System.ComponentModel.DataAnnotations;

namespace ResourceBookingSystem.WebApp.Resource.DTOs;

public sealed class ResourceRequestDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string Location { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int? Capacity { get; set; }

    [Required]
    public Guid ResourceCategoryId { get; set; }
}

public sealed class ResourceStatusUpdateDto
{
    [Required]
    public string Status { get; set; } = null!;
}

public sealed class ResourceResponseDto
{
    public Guid ResourceId { get; init; }
    public string Name { get; init; } = null!;
    public string Location { get; init; } = null!;
    public int? Capacity { get; init; }
    public string Status { get; init; } = null!;
    public Guid ResourceCategoryId { get; init; }
    public string ResourceCategoryName { get; init; } = null!;
}