#pragma warning disable CA1862

using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services.Sorting;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits(
        HabitsQueryParameters queryParams,
        SortMappingProvider sortMappingProvider)
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(queryParams.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{queryParams.Sort}'");
        }
        
        IQueryable<Habit> query = dbContext.Habits;

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
        
        List<HabitDto> habits = await query
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();

        var collectionDto = new HabitsCollectionDto
        {
            Data = habits
        };
        
        return Ok(collectionDto);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabit(string id)
    {
        HabitWithTagsDto? habit = await dbContext.Habits
            .Select(HabitQueries.ProjectToDtoWithTags())
            .FirstOrDefaultAsync(x => x.Id == id);

        if (habit is null)
        {
            return NotFound();
        }
        
        return Ok(habit);
    }
    
    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto dto, IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);

        Habit habit = dto.ToEntity();

        dbContext.Habits.Add(habit);
        
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHabit), new { habit.Id }, habit.ToDto());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto dto)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(x => x.Id == id);

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
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(x => x.Id == id);

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
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(x => x.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
