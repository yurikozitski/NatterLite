using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatterLite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NatterLite.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace NatterLite.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ApplicationContext db;
        private readonly IMemoryCache cache;
        public AdminController(
            UserManager<User> _userManager,
            ApplicationContext context,
            IMemoryCache _memoryCache)
        {
            userManager = _userManager;
            db = context;
            cache = _memoryCache;
        }

        [HttpPost]
        public async Task<IActionResult> BanUser(string userName)
        {
            User user= await userManager.FindByNameAsync(userName);
            if(user==null) return RedirectToAction("Search", "Search");

            user.IsBanned = true;
            await userManager.UpdateAsync(user);

            if (cache.TryGetValue(userName, out User userFromCache))
            {
                cache.Remove(userName);
                cache.Set(userName, user, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });
            }
          
            return new StatusCodeResult(200);
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(string userName)
        {
            User user = await userManager.FindByNameAsync(userName);
            if (user == null) return RedirectToAction("Search", "Search");

            user.IsBanned = false;
            await userManager.UpdateAsync(user);

            if (cache.TryGetValue(userName, out User userFromCache))
            {
                cache.Remove(userName);
                cache.Set(userName, user, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });
            }

            return new StatusCodeResult(200);
        }

        [HttpGet]
        public IActionResult BannedUsers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetBannedUsers(string Name)
        {
            List<UserSearchViewModel> usvmList = new List<UserSearchViewModel>();
            List<User> searchedUsers = new List<User>();

            if (Name != null)
            {
                if (Name.StartsWith("@"))
                {
                    searchedUsers = await db.Users.Where(u => u.UserName==Name&&u.IsBanned).ToListAsync();
                }
                else
                {
                    searchedUsers = await db.Users.Where(u => u.FullName.Contains(Name)&&u.IsBanned).ToListAsync();
                }
            }
            else
            {
                searchedUsers = await db.Users.Where(u =>u.IsBanned).ToListAsync();
            }

            if (searchedUsers.Count != 0)
            {
                foreach (User user in searchedUsers)
                {
                    UserSearchViewModel usvm = new UserSearchViewModel();
                    usvm.UserId = user.Id;
                    usvm.UserName = user.FullName;
                    usvm.UserUniqueName = user.UserName;
                    usvm.UserProfilePicture = user.ProfilePicture;
                    usvm.IsBanned = user.IsBanned;
                    usvmList.Add(usvm);
                }
            }

            return PartialView("BannedUsersSearchResultPartial",usvmList);
        }
    }
}