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
using System.Drawing.Imaging;
using System.Drawing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using NatterLite.Filters;

namespace NatterLite.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly IPicturesProvider picturesProvider;
        private readonly ICountryList countriesProvider;
        private readonly IImageValidator imageValidator;
        private readonly IMemoryCache cache;
        private readonly ApplicationContext db;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        public AccountController(
            ILogger<AccountController> _logger,
            IConfiguration _configuration,
            IPicturesProvider _picturesProvider,
            ICountryList _countriesProvider,
            IImageValidator _imageValidator,
            IMemoryCache _memoryCache,
            ApplicationContext context,
            UserManager<User> _userManager, 
            SignInManager<User> _signInManager,
            RoleManager<IdentityRole> _manager)
        {
            logger = _logger;
            configuration = _configuration;
            picturesProvider = _picturesProvider;
            countriesProvider = _countriesProvider;
            imageValidator = _imageValidator;
            cache = _memoryCache;
            db = context;
            userManager = _userManager;
            signInManager = _signInManager;
            roleManager = _manager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("ChatMenu", "Chat");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel lvm)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u=>u.Email==lvm.Email);
                if (user != null)
                {
                    if (user.IsBanned) 
                    {
                        ModelState.AddModelError(string.Empty, "You were banned by admin");
                        return View();
                    }
                    var result=await signInManager.PasswordSignInAsync(user, lvm.Password, false, false);
                    if (result.Succeeded)
                    {
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
                        catch(Exception ex) 
                        {
                            logger.LogError(ex, "Can't create an user's profile pictire at SignedUsersPics");
                            return RedirectToAction("Error", "Home");
                        }

                        cache.Set(user.UserName, user, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                        });

                        return RedirectToAction("ChatMenu", "Chat");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Password or Login");
                    }
                    
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Password or Login");
                }
            }
            return View(lvm);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ChatMenu", "Chat");
            }
            ViewBag.Countries = countriesProvider.CountryList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel rvm)
        {
            ViewBag.Countries = countriesProvider.CountryList();
            if (ModelState.IsValid)
            {
                User user = new User { 
                    Email = rvm.Email, 
                    FirstName=rvm.FirstName,
                    LastName=rvm.LastName,
                    FullName = rvm.FirstName+" "+rvm.LastName, 
                    UserName = rvm.UniqueName,
                    DateOfBirth=rvm.DateOfBirth,
                    Country=rvm.Country,
                };
                if (rvm.ProfilePicture != null)
                {
                    if (imageValidator.IsImageValid(rvm.ProfilePicture))
                    {
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(rvm.ProfilePicture.OpenReadStream()))
                        {
                            imageData = binaryReader.ReadBytes((int)rvm.ProfilePicture.Length);
                        }
                        user.ProfilePicture = imageData;
                    }
                    else
                    {
                        ModelState.AddModelError("ProfilePicture", "Picture size bigger than 2Mb or has invalid extension");
                        return View(rvm);
                    }
                }
                else
                {
                    try
                    {
                        user.ProfilePicture = picturesProvider.GetDefaultPicture(configuration["PicturesPaths:DefaultProfilePicturePath"]);
                    }
                    catch(Exception ex)
                    {
                        logger.LogError(ex, "Can't read default profile picture");
                        return Content($"{ex.Message}");
                    }
                    
                }
                if (rvm.BackgroundPicture != null)
                {
                    if (imageValidator.IsImageValid(rvm.BackgroundPicture))
                    {
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(rvm.BackgroundPicture.OpenReadStream()))
                        {
                            imageData = binaryReader.ReadBytes((int)rvm.BackgroundPicture.Length);
                        }
                        user.BackgroundPicture = imageData;
                    }
                    else
                    {
                        ModelState.AddModelError("BackgroundPicture", "Picture size bigger than 2Mb or has invalid extension");
                        return View(rvm);
                    }
                }
                else
                {
                    try
                    {
                        user.BackgroundPicture = picturesProvider.GetDefaultPicture(configuration["PicturesPaths:DefaultBackgroundPicturePath"]);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Can't read default background picture");
                        return Content($"{ex.Message}");
                    }

                }
                
                var result = await userManager.CreateAsync(user, rvm.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "user");
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
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Can't create an user's profile pictire at SignedUsersPics");
                        return RedirectToAction("Error", "Home");
                    }

                    await signInManager.SignInAsync(user, false);

                    cache.Set(user.UserName, user, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                    });

                    return RedirectToAction("ChatMenu", "Chat");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(rvm);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            await signInManager.SignOutAsync();

            if (cache.TryGetValue(User.Identity.Name, out User user)) cache.Remove(User.Identity.Name);

            if (System.IO.File.Exists(@$"C:\MyApps\NatterLite\wwwroot\SignedUsersPics\{User.Identity.Name}.jpg"))
            {
                System.IO.File.Delete(@$"C:\MyApps\NatterLite\wwwroot\SignedUsersPics\{User.Identity.Name}.jpg");
            }
            return RedirectToAction("Login", "Account");
        }
    }
}
