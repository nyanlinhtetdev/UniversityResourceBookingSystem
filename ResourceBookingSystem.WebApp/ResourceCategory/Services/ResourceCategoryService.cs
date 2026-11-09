using Microsoft.EntityFrameworkCore;
using ResourceBookingSystem.Database.AppDbContextModels;
using ResourceBookingSystem.WebApp.ResourceCategory.DTOs;
using ResourceBookingSystem.WebApp.ResourceCategory.ServiceInterfaces;
using ResourceCategoryEntity = ResourceBookingSystem.Database.AppDbContextModels.ResourceCategory;

namespace ResourceBookingSystem.WebApp.ResourceCategory.Services;

public sealed class ResourceCategoryService(AppDbContext dbContext)
    : IResourceCategoryService
{
    public async Task<IReadOnlyList<ResourceCategoryResponseDto>> GetAllAsync()
    {
        return await dbContext.ResourceCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new ResourceCategoryResponseDto
            {
                ResourceCategoryId = category.ResourceCategoryId,
                Name = category.Name
            })
            .ToListAsync();
    }

    public async Task<ResourceCategoryResult> CreateAsync(
        ResourceCategoryRequestDto request)
    {
        var name = request.Name.Trim();

        if (await NameExistsAsync(name))
        {
            return new(ResourceCategoryMutationStatus.NameAlreadyExists);
        }

        var category = new ResourceCategoryEntity
        {
            Name = name
        };

        await dbContext.ResourceCategories.AddAsync(category);
        await dbContext.SaveChangesAsync();

        return new(
            ResourceCategoryMutationStatus.Success,
            ToResponse(category));
    }

    public async Task<ResourceCategoryResult> UpdateAsync(
        Guid categoryId,
        ResourceCategoryRequestDto request)
    {
        var category = await dbContext.ResourceCategories
            .FirstOrDefaultAsync(item =>
                item.ResourceCategoryId == categoryId);

        if (category is null)
        {
            return new(ResourceCategoryMutationStatus.NotFound);
        }

        var name = request.Name.Trim();

        if (await NameExistsAsync(name, categoryId))
        {
            return new(ResourceCategoryMutationStatus.NameAlreadyExists);
        }

        category.Name = name;
        await dbContext.SaveChangesAsync();

        return new(
            ResourceCategoryMutationStatus.Success,
            ToResponse(category));
    }

    public async Task<ResourceCategoryResult> DeleteAsync(Guid categoryId)
    {
        var category = await dbContext.ResourceCategories
            .Include(item => item.Resources)
            .FirstOrDefaultAsync(item =>
                item.ResourceCategoryId == categoryId);

        if (category is null)
        {
            return new(ResourceCategoryMutationStatus.NotFound);
        }

        if (category.Resources.Count > 0)
        {
            return new(ResourceCategoryMutationStatus.HasResources);
        }

        dbContext.ResourceCategories.Remove(category);
        await dbContext.SaveChangesAsync();

        return new(ResourceCategoryMutationStatus.Success);
    }

    private Task<bool> NameExistsAsync(
        string name,
        Guid? excludedCategoryId = null)
    {
        var normalizedName = name.ToLower();

        return dbContext.ResourceCategories.AnyAsync(category =>
            category.Name.ToLower() == normalizedName &&
            (excludedCategoryId == null ||
             category.ResourceCategoryId != excludedCategoryId));
    }

    private static ResourceCategoryResponseDto ToResponse(
        ResourceCategoryEntity category)
    {
        return new ResourceCategoryResponseDto
        {
            ResourceCategoryId = category.ResourceCategoryId,
            Name = category.Name
        };
    }
}