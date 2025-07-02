using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Auth;
using DevHabit.Api.Dtos.Users;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace DevHabit.Api.Controllers;

[Route("auth")]
[ApiController]
[AllowAnonymous]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationIdentityDbContext identityDbContext,
    ApplicationDbContext applicationDbContext,
    TokenProvider tokenProvider,
    IOptions<JwtAuthOptions> jwtAuthOptions) 
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AccessTokensDto>> Register(RegisterUserDto dto)
    {
        await using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync();
        applicationDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());
        
        IdentityUser identityUser = new(dto.Name) { Email = dto.Email };
        
        IdentityResult createUserResult = await userManager
            .CreateAsync(identityUser, dto.Password);

        if (!createUserResult.Succeeded)
        {
            return Problem(
                detail: "Unable to register user, please try again.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>()
                {
                    { "errors",  createUserResult.Errors.ToDictionary(x => x.Code, x => x.Description) }
                });
        }

        IdentityResult addToRoleResult = await userManager.AddToRoleAsync(identityUser, Roles.Member);
        
        if (!addToRoleResult.Succeeded)
        {
            return Problem(
                detail: "Unable to register user, please try again.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>()
                {
                    { "errors", addToRoleResult.Errors.ToDictionary(x => x.Code, x => x.Description) }
                });
        }
        
        User user = dto.ToEntity(identityUser.Id); 

        applicationDbContext.Users.Add(user);
        await applicationDbContext.SaveChangesAsync();

        IList<string> userRoles = await userManager.GetRolesAsync(identityUser);

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email, userRoles);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtAuthOptions.Value.RefreshTokenExpirationInDays),
            User = identityUser
        };
        
        identityDbContext.Add(refreshToken);
        await identityDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(accessTokens);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokensDto>> Login(LoginUserDto dto)
    {
        IdentityUser? identityUser = await userManager.FindByEmailAsync(dto.EmailAddress);

        if (identityUser is null || !await userManager.CheckPasswordAsync(identityUser, dto.Password))
        {
            return Problem(
                detail: "Invalid username or password.",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        
        IList<string> userRoles = await userManager.GetRolesAsync(identityUser);

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email!, userRoles);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);
        
        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtAuthOptions.Value.RefreshTokenExpirationInDays),
            User = identityUser
        };
        
        identityDbContext.Add(refreshToken);
        await identityDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AccessTokensDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        RefreshToken? refreshToken = await identityDbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshTokenDto.RefreshToken);
    
        if (refreshToken is null || refreshToken.ExpiresAtUtc < DateTime.UtcNow)
        {
            return Unauthorized();
        }
        
        IList<string> userRoles = await userManager.GetRolesAsync(refreshToken.User);
        
        var tokenRequest = new TokenRequest(refreshToken.User.Id, refreshToken.User.Email!, userRoles);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);
    
        refreshToken.Token = accessTokens.RefreshToken;
        refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtAuthOptions.Value.RefreshTokenExpirationInDays);
    
        await identityDbContext.SaveChangesAsync();
    
        return Ok(accessTokens);
    }
}
