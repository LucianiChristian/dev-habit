﻿using System.Linq.Expressions;
using DevHabit.Api.Entities;

namespace DevHabit.Api.Dtos.Tags;

public static class TagQueries
{
    public static Expression<Func<Tag, TagDto>> ProjectToDto()
    {
        return tag => new TagDto
        {
            Id = tag.Id, 
            Name = tag.Name, 
            Description = tag.Description, 
            CreatedAtUtc = tag.CreatedAtUtc, 
            UpdatedAtUtc = tag.UpdatedAtUtc 
        };
    }
}
