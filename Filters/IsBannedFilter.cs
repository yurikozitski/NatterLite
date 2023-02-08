using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using NatterLite.Models;
using Microsoft.Extensions.Caching.Memory;

namespace NatterLite.Filters
{
    public class IsBannedFilter : Attribute, IAsyncResourceFilter
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IMemoryCache cache;
        public IsBannedFilter(
            UserManager<User> _userManager,
            SignInManager<User> _signInManager,
            IMemoryCache _memoryCache)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            cache = _memoryCache;
        }
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
                                    ResourceExecutionDelegate next)
        {
            if(!context.HttpContext.User.Identity.IsAuthenticated) await next();

            User user;

            if (!cache.TryGetValue(context.HttpContext.User.Identity.Name, out user))
            {
                user = await userManager.FindByNameAsync(context.HttpContext.User.Identity.Name);
            }
            
            if (user.IsBanned)
            {
                context.Result = new RedirectToActionResult("Logout", "Account", new {  });
            }
            else
            {
                await next();
            }
        }
    }
}
