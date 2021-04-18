using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Server.Data;
using MasterCsharpHosted.Shared;
using Microsoft.EntityFrameworkCore;

namespace MasterCsharpHosted.Server.Controllers
{
    [Route("api/appUser")]
    [ApiController]
    public class AppUserController : ControllerBase
    {
        private readonly IDbContextFactory<AppUserContext> _userContext;

        public AppUserController(IDbContextFactory<AppUserContext> userContext)
        {
            _userContext = userContext;
        }

        [HttpGet("getUser/{userName}")]
        public async Task<IActionResult> GetUserData(string userName)
        {
            await using var context = _userContext.CreateDbContext();
            var user = await context.AppUsers.FirstOrDefaultAsync(x => x.UserName == userName);
            return user == null
                ? new OkObjectResult(new AppUser {UserName = "NONE"})
                : new OkObjectResult(user);
        }

        [HttpPost("addUser")]
        public async Task<IActionResult> AddUser([FromBody]AppUser user)
        {
            try
            {
                await using var context = _userContext.CreateDbContext();
                await context.AddAsync(user);
                await context.SaveChangesAsync();
                return new OkObjectResult(user);
            }
            catch (Exception ex)
            {
                string errorString = $"{ex.Message}\nStack: {ex.StackTrace}\nInner: {ex.InnerException}";
                Console.WriteLine(errorString);
                return new BadRequestObjectResult(errorString);
            }

        }

        [HttpPost("updateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] AppUser user)
        {
            try
            {
                await using var context = _userContext.CreateDbContext();
                var appUser = await context.AppUsers.FirstOrDefaultAsync(x => x.UserName == user.UserName);
                appUser.Snippets = user.Snippets;
                appUser.CompletedChallenges = user.CompletedChallenges;
                await context.SaveChangesAsync();
                return new OkObjectResult("Success!");
            }
            catch(Exception ex)
            {
                string errorString = $"{ex.Message}\nStack: {ex.StackTrace}\nInner: {ex.InnerException}";
                Console.WriteLine(errorString);
                return new BadRequestObjectResult(errorString);
            }
        }
    }
}
