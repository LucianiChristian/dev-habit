using DevHabit.Api.Dtos.Auth;
using DevHabit.Api.Entities;

namespace DevHabit.Api.Dtos.Users;

internal static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto, string identityId)
    {
        DateTime utcNow = DateTime.UtcNow;
        
        return new User
        {
            Id = $"u_{Guid.CreateVersion7()}",
            Email = dto.Email, 
            Name = dto.Name, 
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow, 
            IdentityId = identityId
        };
    } 
}
