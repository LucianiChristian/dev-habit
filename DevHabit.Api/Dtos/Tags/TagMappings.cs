﻿using DevHabit.Api.Entities;

namespace DevHabit.Api.Dtos.Tags;

public static class TagMappings
{
    public static Tag ToEntity(this CreateTagDto dto)
    {
        return new Tag
        {
            Id = $"t_{Guid.CreateVersion7()}",
            Name = dto.Name,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id, 
            Name = tag.Name, 
            Description = tag.Description, 
            CreatedAtUtc = tag.CreatedAtUtc, 
            UpdatedAtUtc = tag.UpdatedAtUtc 
        };
    }

    public static void UpdateFromDto(this Tag tag, UpdateTagDto dto)
    {
        tag.Name = dto.Name;
        tag.Description = dto.Description;
        
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }
}
