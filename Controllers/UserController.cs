using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatterLite.Models;
using NatterLite.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using NatterLite.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace NatterLite.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(IsBannedFilter))]
    public class UserController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ApplicationContext db;
        private readonly ICountryList countriesProvider;
        private readonly IImageValidator imageValidator;
        private readonly IMemoryCache cache;
        public UserController(
            ApplicationContext context,
            IImageValidator _imageValidator,
            ICountryList _countriesProvider,
            IMemoryCache _memoryCache,
            UserManager<User> _userManager)
        {
            db = context;
            userManager = _userManager;
            countriesProvider = _countriesProvider;
            imageValidator = _imageValidator;
            cache = _memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> SeeProfile(string UserUniqueName)
        {
            User user;
            bool IsCurrentUser=false;
            if (UserUniqueName != null)
            {
                user = await userManager.FindByNameAsync(UserUniqueName);
            }
            else
            {
                user = await userManager.GetUserAsync(this.User);
                IsCurrentUser = true;
            }

            UserProfileViewModel upvm = new UserProfileViewModel();
            upvm.UserFullName = user.FullName;
            upvm.UserUniqueName = user.UserName;
            upvm.UserStatus = user.Status;
            upvm.UserDateOfBirth = user.DateOfBirth.ToString("dd.MM.yyyy");
            upvm.UserCountry = user.Country;
            upvm.UserProfilePicture = user.ProfilePicture;
            upvm.UserBackgroundPicture = user.BackgroundPicture;
            upvm.IsThisCurrentUser = IsCurrentUser;

            return View(upvm);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            ViewBag.Countries = countriesProvider.CountryList();
            User user;

            if (!cache.TryGetValue(User.Identity.Name, out user))
            {
                user = await userManager.GetUserAsync(this.User);

            }
            
            EditViewModel evm = new EditViewModel();
            evm.FirstName = user.FirstName;
            evm.LastName = user.LastName;
            evm.UniqueName = user.UserName;
            evm.Email = user.Email;
            evm.Country = user.Country;
            evm.DateOfBirth = user.DateOfBirth;

            return View(evm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditViewModel evm)
        {
            ViewBag.Countries = countriesProvider.CountryList();
            User user=await userManager.GetUserAsync(this.User);

            bool IsUserUniqueNameCanged = user.UserName != evm.UniqueName;

            if (ModelState.IsValid)
            {
                user.FirstName= evm.FirstName;
                user.LastName= evm.LastName;
                user.FullName = evm.FirstName + " " + evm.LastName;
                user.UserName=evm.UniqueName;
                user.Email=evm.Email;
                user.Country=evm.Country;
                user.DateOfBirth=evm.DateOfBirth;
                if (evm.OldPassword != null && evm.NewPassword != null) 
                {
                    var result=await userManager.ChangePasswordAsync(user, evm.OldPassword, evm.NewPassword);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("OldPassword", "Old password is incorrect!");
                        return View(evm);
                    }
                }
                if (evm.ProfilePicture != null)
                {
                    if (imageValidator.IsImageValid(evm.ProfilePicture))
                    {
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(evm.ProfilePicture.OpenReadStream()))
                        {
                            imageData = binaryReader.ReadBytes((int)evm.ProfilePicture.Length);
                        }
                        user.ProfilePicture = imageData;
                    }
                    else
                    {
                        ModelState.AddModelError("ProfilePicture", "Picture size bigger than 2Mb or has invalid extension");
                        return View(evm);
                    }
                }
                if (evm.BackgroundPicture != null)
                {
                    if (imageValidator.IsImageValid(evm.BackgroundPicture))
                    {
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(evm.BackgroundPicture.OpenReadStream()))
                        {
                            imageData = binaryReader.ReadBytes((int)evm.BackgroundPicture.Length);
                        }
                        user.BackgroundPicture = imageData;
                    }
                    else
                    {
                        ModelState.AddModelError("BackgroundPicture", "Picture size bigger than 2Mb or has invalid extension");
                        return View(evm);
                    }
                }
                try
                {
                    string userPicturePath = @$"C:\MyApps\NatterLite\wwwroot\SignedUsersPics\{user.UserName}.jpg";
                    using (Image image = Image.FromStream(new MemoryStream(user.ProfilePicture)))
                    {
                        image.Save(userPicturePath, ImageFormat.Jpeg);
                    }
                    HttpContext.Response.Cookies.Append("userPicturePath", $"{user.UserName}.jpg");
                    HttpContext.Response.Cookies.Append("userName", $"{user.FullName}");
                }
                catch
                {
                    return RedirectToAction("Error", "Home");
                }

                if (cache.TryGetValue(User.Identity.Name, out User userFromCache))
                {
                    cache.Remove(User.Identity.Name);
                    cache.Set(user.UserName, user, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                    });
                }
                await userManager.UpdateAsync(user);

                if (IsUserUniqueNameCanged) return RedirectToAction("Logout", "Account");

            }
         
            return View(evm);
        }

        [HttpPut]
        public async Task<IActionResult> AddToBlackList(string CompanionUniqueName)
        {
            User userToBlacklist = await userManager.FindByNameAsync(CompanionUniqueName);
            User currentUser = await db.Users.Include(u => u.BlackList).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser.BlackList.Exists(u => u.UserName == CompanionUniqueName))
            {
                return new StatusCodeResult(204);
            }
            else
            {
                currentUser.BlackList.Add(userToBlacklist);
                await db.SaveChangesAsync();
                return new StatusCodeResult(200);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SeeBlackList()
        {
            User currentUser = await db.Users.Include(u => u.BlackList).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<UserBlackListViewModel> ublvmList = new List<UserBlackListViewModel>();
            if (currentUser.BlackList.Count!=0)
            {
                foreach(User user in currentUser.BlackList)
                {
                    UserBlackListViewModel ublvm = new UserBlackListViewModel();
                    ublvm.UserName = user.FullName;
                    ublvm.UserUniqueName = user.UserName;
                    ublvm.UserProfilePicture = user.ProfilePicture;
                    ublvmList.Add(ublvm);
                }
            }
            return View(ublvmList);
            
        }

        [HttpPut]
        public async Task<IActionResult> RemoveFromBlackList(string userName)
        {
            User currentUser = await db.Users.Include(u => u.BlackList).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            User userToRemove = currentUser.BlackList.Find(u => u.UserName == userName);
            currentUser.BlackList.Remove(userToRemove);
            await db.SaveChangesAsync();

            List<UserBlackListViewModel> ublvmList = new List<UserBlackListViewModel>();
            if (currentUser.BlackList.Count != 0)
            {
                foreach (User user in currentUser.BlackList)
                {
                    UserBlackListViewModel ublvm = new UserBlackListViewModel();
                    ublvm.UserName = user.FullName;
                    ublvm.UserUniqueName = user.UserName;
                    ublvm.UserProfilePicture = user.ProfilePicture;
                    ublvmList.Add(ublvm);
                }
            }
            return PartialView("SeeBlackList",ublvmList);
        }

        [HttpPut]
        public async Task<IActionResult> SetUserStatus(string status)
        {
            User currentUser = await userManager.GetUserAsync(this.User);
            if (status == "online")
            {
                currentUser.Status = status;
                await userManager.UpdateAsync(currentUser);
                return new StatusCodeResult(200);
            }
            else
            {
                currentUser.Status = "last seen "+DateTime.Now.ToString("dd.MM.yy,HH:mm");
                await userManager.UpdateAsync(currentUser);
                return new StatusCodeResult(200);
            }
            
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCompanionUserStatus(string userName)
        {
            User companionUser = await userManager.FindByNameAsync(userName);
            return Content(companionUser.Status);
        }
    }
}