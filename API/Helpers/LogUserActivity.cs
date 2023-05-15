using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var requestContext = await next();
            if(!requestContext.HttpContext.User.Identity.IsAuthenticated) return;

            var userId = requestContext.HttpContext.User.GetUserId();
            var repo = requestContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var user = await repo.GetUserByIdAsync(userId); 
            user.LastActive = DateTime.UtcNow;
            await repo.SaveUserAsync();
        }
    }
}