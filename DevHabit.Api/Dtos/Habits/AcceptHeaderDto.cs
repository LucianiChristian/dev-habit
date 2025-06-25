using DevHabit.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Dtos.Habits;

public record AcceptHeaderDto
{
    [FromHeader(Name = "Accept")]
    public string? Accept { get; init; }

    public bool IncludeLinks => Accept is 
        CustomMediaTypeNames.Application.HateoasJson or 
        CustomMediaTypeNames.Application.HateoasJsonV1 or
        CustomMediaTypeNames.Application.HateoasJsonV2;
}
