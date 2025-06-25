using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Auth;
using DevHabit.Api.Dtos.Users;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DevHabit.Api.Controllers;

[Route("auth")]
[ApiController]
[AllowAnonymous]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationIdentityDbContext identityDbContext,
    ApplicationDbContext applicationDbContext) 
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        await using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync();
        applicationDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());
        
        IdentityUser identityUser = new(dto.Name) { Email = dto.Email };
        
        IdentityResult result = await userManager
            .CreateAsync(identityUser, dto.Password);

        if (!result.Succeeded)
        {
            return Problem(
                detail: "Unable to register user, please try again.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>()
                {
                    { "errors",  result.Errors.ToDictionary(x => x.Code, x => x.Description) }
                });
        }
        
        User user = dto.ToEntity(identityUser.Id); 

        applicationDbContext.Users.Add(user);
        await applicationDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(user.Id);
    }
}
