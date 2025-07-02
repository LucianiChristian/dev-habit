namespace DevHabit.Api.Dtos.Auth;

public sealed record TokenRequest(string UserId, string Email, IEnumerable<string> Roles);
