using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Tags;
using DevHabit.Api.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("tags")]
public class TagsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags()
    {
        List<TagDto> tags = await dbContext.Tags
            .Select(TagQueries.ProjectToDto())
            .ToListAsync();

        var tagsCollection = new TagsCollectionDto
        {
            Items = tags
        };

        return Ok(tagsCollection);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(string id)
    {
        TagDto? tag = await dbContext.Tags
            .Select(TagQueries.ProjectToDto())
            .FirstOrDefaultAsync(x => x.Id == id);

        if (tag is null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto dto, IValidator<CreateTagDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
        Tag tag = dto.ToEntity();

        bool nameAlreadyExists = await dbContext.Tags.AnyAsync(x => x.Name == dto.Name);

        if (nameAlreadyExists)
        {
            return Problem(
                detail: $"The tag '{tag.Name}' already exists",
                statusCode: StatusCodes.Status409Conflict);
        }

        dbContext.Tags.Add(tag);

        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag.ToDto());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(UpdateTagDto dto, string id)
    {
        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(x => x.Id == id);

        if (tag is null)
        {
            return NotFound();
        }
        
        bool nameAlreadyExists = await dbContext.Tags
            .Where(x => x.Id != id)
            .AnyAsync(x => x.Name == dto.Name);
        
        if (nameAlreadyExists)
        {
            return Conflict($"The tag '{dto.Name}' already exists");
        }
        
        tag.UpdateFromDto(dto);

        await dbContext.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(x => x.Id == id);

        if (tag is null)
        {
            return NotFound();
        }

        dbContext.Tags.Remove(tag);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
