﻿#pragma warning disable CA1862

using System.Dynamic;
using System.Net.Mime;
using Asp.Versioning;
using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Common;
using DevHabit.Api.Dtos.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Authorize(Roles = Roles.Member)]
[ApiController]
[Route("habits")]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.JsonV2,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1,
    CustomMediaTypeNames.Application.HateoasJsonV2)]
public sealed class HabitsController(ApplicationDbContext dbContext, LinkService linkService, UserContext userContext) 
    : ControllerBase
{
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json, CustomMediaTypeNames.Application.HateoasJson)]
    public async Task<IActionResult> GetHabits(
        HabitsQueryParameters queryParams,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }
        
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(queryParams.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{queryParams.Sort}'");
        }

        if (!dataShapingService.Validate<HabitDto>(queryParams.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{queryParams.Fields}'");
        }
        
        IQueryable<Habit> query = dbContext.Habits.Where(x => x.UserId == userId);

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        if (queryParams.Search is not null)
        {
            query = query.Where(x => 
                    x.Name.ToLower().Contains(queryParams.Search) ||
                    x.Description != null && x.Description.ToLower().Contains(queryParams.Search));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            query = query.Where(x =>
                x.Name.ToLower().Contains(queryParams.Search) ||
                x.Description != null && x.Description.ToLower().Contains(queryParams.Search));
        }

        if (queryParams.Status is not null)
        {
            query = query.Where(x => x.Status == queryParams.Status);
        }

        if (queryParams.Type is not null)
        {
            query = query.Where(x => x.Type == queryParams.Type);
        }

        query = query.ApplySort(queryParams.Sort, sortMappings);

        int totalCount = await query.CountAsync();

        List<HabitDto> habits = await query
            .Skip(queryParams.PageSize * (queryParams.Page - 1))
            .Take(queryParams.PageSize)
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeMany(
                habits,
                queryParams.Fields,
                queryParams.IncludeLinks ? h => CreateLinksForHabit(h.Id, queryParams.Fields) : null),
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount
        };

        if (queryParams.IncludeLinks)
        {
            paginationResult = paginationResult with
            {
                Links = CreateLinksForHabits(queryParams, paginationResult.HasNextPage, paginationResult.HasPreviousPage)
            };
        }
        
        return Ok(paginationResult);
    }
    
    [HttpGet("{id}")]
    [MapToApiVersion(1.0)]
    public async Task<IActionResult> GetHabit(
        HabitQueryParameters queryParams,
        DataShapingService dataShapingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }
        
        if (!dataShapingService.Validate<HabitWithTagsDto>(queryParams.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{queryParams.Fields}'");
        }
        
        HabitWithTagsDto? habit = await dbContext
            .Habits
            .Where(x => x.Id == queryParams.Id && x.UserId == userId)
            .Select(HabitQueries.ProjectToDtoWithTags())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        ExpandoObject shapedHabitDto = dataShapingService.Shape(habit, queryParams.Fields);

        if (queryParams.IncludeLinks)
        {
            List<LinkDto> links = CreateLinksForHabit(queryParams.Id, queryParams.Fields);

            shapedHabitDto.TryAdd("links", links);
        }
        
        return Ok(shapedHabitDto);
    }
    
    [HttpGet("{id}")]
    [MapToApiVersion(2.0)]
    public async Task<IActionResult> GetHabitV2(
        HabitQueryParameters queryParams,
        DataShapingService dataShapingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }
        
        if (!dataShapingService.Validate<HabitWithTagsDtoV2>(queryParams.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{queryParams.Fields}'");
        }
        
        HabitWithTagsDtoV2? habit = await dbContext
            .Habits
            .Where(x => x.UserId == userId)
            .Select(HabitQueries.ProjectToDtoWithTagsV2())
            .FirstOrDefaultAsync(x => x.Id == queryParams.Id);

        if (habit is null)
        {
            return NotFound();
        }

        ExpandoObject shapedHabitDto = dataShapingService.Shape(habit, queryParams.Fields);

        if (queryParams.IncludeLinks)
        {
            List<LinkDto> links = CreateLinksForHabit(queryParams.Id, queryParams.Fields);

            shapedHabitDto.TryAdd("links", links);
        }
        
        return Ok(shapedHabitDto);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto dto, IValidator<CreateHabitDto> validator)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }
        
        await validator.ValidateAndThrowAsync(dto);

        Habit habit = dto.ToEntity(userId);

        dbContext.Habits.Add(habit);
        
        await dbContext.SaveChangesAsync();

        List<LinkDto> links = CreateLinksForHabit(habit.Id, null);

        HabitDto habitDto = habit.ToDto(links);

        return CreatedAtAction(nameof(GetHabit), new { habit.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto dto)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }
        
        Habit? habit = await dbContext.Habits
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(dto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }
        
        Habit? habit = await dbContext.Habits
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (habit is null)
        {
            return NotFound();
        }

        HabitDto habitDto = habit.ToDto();
        
        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }
        
        Habit? habit = await dbContext.Habits
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (habit is null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
    
    private List<LinkDto> CreateLinksForHabits(
        HabitsQueryParameters parameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links = 
        [
            linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new
            {
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "next-page", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "previous-page", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }
        
        return links;
    }

    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
    {
        return [
            linkService.Create(nameof(GetHabit), "self", HttpMethods.Get, new { id, fields }),
            linkService.Create(nameof(UpdateHabit), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(PatchHabit), "partial-update", HttpMethods.Patch, new { id }),
            linkService.Create(nameof(DeleteHabit), "delete", HttpMethods.Delete, new { id }),
            linkService.Create(nameof(HabitTagsController.UpsertHabitTags), "upsert-tags", HttpMethods.Put, new { habitId = id }, "HabitTags"),
        ];
    }
}
