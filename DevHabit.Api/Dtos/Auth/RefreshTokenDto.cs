namespace DevHabit.Api.Dtos.Auth;

public sealed record RefreshTokenDto
{
    public required string RefreshToken { get; init; }
}
