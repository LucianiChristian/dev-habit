using DevHabit.Api.Database;
using DevHabit.Api.Dtos.GitHub;
using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Services;

public sealed class GitHubAccessTokenService(ApplicationDbContext dbContext)
{
    public async Task StoreAsync(
        string userId,
        StoreGitHubAccessTokenDto accessTokenDto,
        CancellationToken cancellationToken = default)
    {
        GitHubAccessToken? existingAccessToken = await GetAccessTokenAsync(userId, cancellationToken);
        
        DateTime expirationDate = DateTime.UtcNow.AddDays(accessTokenDto.ExpiresInDays);

        if (existingAccessToken is not null)
        {
            existingAccessToken.Token = accessTokenDto.AccessToken;
            existingAccessToken.ExpiresAtUtc = expirationDate;
        }
        else
        {
            dbContext.GitHubAccessTokens.Add(new GitHubAccessToken
            {
                Id = $"gh_{Guid.CreateVersion7()}",
                UserId = userId,
                Token = accessTokenDto.AccessToken,
                ExpiresAtUtc = expirationDate,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        GitHubAccessToken? gitHubAccessToken = await GetAccessTokenAsync(userId, cancellationToken);

        return gitHubAccessToken?.Token;
    }
    
    public async Task RevokeAsync(string userId, CancellationToken cancellationToken = default)
    {
        GitHubAccessToken? gitHubAccessToken = await GetAccessTokenAsync(userId, cancellationToken);

        if (gitHubAccessToken is null)
        {
            return;
        }

        dbContext.GitHubAccessTokens.Remove(gitHubAccessToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<GitHubAccessToken?> GetAccessTokenAsync(string userId, CancellationToken cancellationToken)
    {
        return dbContext.GitHubAccessTokens.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}
