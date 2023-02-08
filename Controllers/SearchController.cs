using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatterLite.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using NatterLite.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using NatterLite.Filters;

namespace NatterLite.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(IsBannedFilter))]
    public class SearchController : Controller
    {
        private readonly ICountryList countriesProvider;
        private readonly ApplicationContext db;
        private readonly UserManager<User> userManager;
        public SearchController(
            ICountryList _countriesProvider,
            ApplicationContext context,
            UserManager<User> _userManager)
        {
            countriesProvider = _countriesProvider;
            db = context;
            userManager = _userManager;
        }

        [HttpGet]
        public IActionResult Search()
        {
            ViewBag.Countries = countriesProvider.CountryList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchResult(string Name,int AgeFrom, int AgeTo, string Country)
        {
            var currentUserFromManager = await userManager.GetUserAsync(this.User);
            ViewBag.Countries = countriesProvider.CountryList();
            bool IsNameFailed=false;
            List<User> userList = new List<User>();
            if (Name != null)
            {
                if (Name.StartsWith("@"))
                {
                    var user = await db.Users.FirstOrDefaultAsync(u=>u.UserName==Name);
                    if (user != null)
                    {
                        userList.Add(user);
                    }
                    else
                    {
                        IsNameFailed = true;
                    }
                }
                else
                {
                    var users = await userManager.Users.Where(u => u.FullName.Contains(Name)).ToListAsync();
                    if (users.Count!=0)
                    {
                        foreach (var User in users)
                        {
                            userList.Add(User);
                        }
                    }
                    else
                    {
                        IsNameFailed = true;
                    }
                }
            }
            bool IsAgeFromFailed = false;
            bool IsAgeToFailed = false;
            
            if (AgeFrom > AgeTo&&AgeTo!=0) (AgeFrom, AgeTo) = (AgeTo, AgeFrom);
            if (AgeFrom != 0&&!IsNameFailed)
            {
                DateTime now = DateTime.Now;
                DateTime searchedDate = now.AddYears(-AgeFrom);
                if (userList.Count!=0)
                {
                    userList = userList.Where(u => u.DateOfBirth <= searchedDate).ToList();
                }
                else
                {
                    userList = await userManager.Users.Where(u => u.DateOfBirth <= searchedDate).ToListAsync();
                }
                if (userList.Count == 0) IsAgeFromFailed = true;
            }
            if (AgeTo != 0 && !IsNameFailed)
            {
                DateTime now = DateTime.Now;
                DateTime searchedDate = now.AddYears(-AgeTo);
                if (userList.Count != 0)
                {
                    userList = userList.Where(u => u.DateOfBirth >= searchedDate).ToList();
                }
                else
                {
                    userList = await userManager.Users.Where(u => u.DateOfBirth >= searchedDate).ToListAsync();
                }
                if (userList.Count == 0) IsAgeToFailed = true;
            }
            if (Country != "NoCountry" && !IsNameFailed&&!IsAgeFromFailed&&!IsAgeToFailed)
            {
                if (userList.Count != 0)
                {
                    userList = userList.Where(u => u.Country==Country).ToList();
                }
                else
                {
                    userList = await userManager.Users.Where(u => u.Country == Country).ToListAsync();
                }
            }
            List<UserSearchViewModel> usvmList = new List<UserSearchViewModel>();
            if (userList != null)
            {
                foreach(var user in userList)
                {
                    if (user.UserName!=User.Identity.Name)
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
            }
            return PartialView("SearchResultPartial", usvmList);
        }
    }
}
