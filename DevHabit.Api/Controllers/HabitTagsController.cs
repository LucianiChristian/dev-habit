using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Habits;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Authorize]
[ApiController]
[Route("habits/{habitId}/tags")]
public class HabitTagsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPut]
    public async Task<ActionResult> UpsertHabitTags(string habitId, UpsertHabitTagsDto dto)
    {
        Habit? habit = await dbContext.Habits
            .Include(x => x.HabitTags)
            .FirstOrDefaultAsync(x => x.Id == habitId);

        if (habit is null)
        {
            return NotFound();
        }
        
        // Determine if there was no change
        var currentTagIds = habit.HabitTags.Select(x => x.TagId).ToHashSet();
        if (currentTagIds.SetEquals(dto.TagIds))
        {
            return NoContent();
        }

        int numberOfMatchingTagsInDatabase = await dbContext.Tags
            .CountAsync(x => dto.TagIds.Contains(x.Id));

        if (numberOfMatchingTagsInDatabase != dto.TagIds.Count)
        {
            return BadRequest("One or more tag ids are invalid.");
        }

        DateTime now = DateTime.UtcNow;

        // remove any existing habit tags that aren't in the dto
        habit.HabitTags.RemoveAll(x => !dto.TagIds.Contains(x.TagId));
        
        // add dto habit tags that didn't already exist
        IEnumerable<string> tagIdsToAdd = dto.TagIds.Except(currentTagIds);

        IEnumerable<HabitTag> habitTagsToAdd = tagIdsToAdd.Select(x => new HabitTag
        {
            HabitId = habit.Id,
            TagId = x,
            CreatedAtUtc = now
        });

        habit.HabitTags.AddRange(habitTagsToAdd);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteTagFromHabit(string habitId, string tagId)
    {
        HabitTag? habitTag = await dbContext.HabitTags
            .FirstOrDefaultAsync(x => x.HabitId == habitId && x.TagId == tagId);

        if (habitTag is null)
        {
            return NotFound();
        }

        dbContext.HabitTags.Remove(habitTag);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
