using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NatterLite.Models;
using NatterLite.Services;
using System.IO;

namespace NatterLite.Initializers
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IPicturesProvider _picturesProvider)
        {
            string adminEmail = "admin@gmail.com";
            string password = "admin123456789";
            string uniqueName = "@admin";
            string firstName = "Super";
            string lastName = "Admin";
            string fullName = firstName + " " + lastName;
            string country = "Ukraine";

            byte[] profilePicture = _picturesProvider
                .GetDefaultPicture(@$"{Directory.GetCurrentDirectory()}\wwwroot\Images\DefaultUsersPictures\admin.jpg");
            byte[] backgroundPicture = _picturesProvider
                .GetDefaultPicture(@$"{Directory.GetCurrentDirectory()}\wwwroot\Images\DefaultUsersPictures\DefaultBackgroundPicture.jpg");

            DateTime dateOfBirth = new DateTime(1991,04,21);
            if (await roleManager.FindByNameAsync("admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }
            if (await roleManager.FindByNameAsync("user") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("user"));
            }
            if (await userManager.FindByNameAsync(uniqueName) == null)
            {
                User admin = new User {
                    Email = adminEmail, 
                    UserName = uniqueName,
                    FirstName=firstName,
                    LastName=lastName,
                    FullName=fullName,
                    Country=country,
                    DateOfBirth=dateOfBirth,
                    ProfilePicture=profilePicture,
                    BackgroundPicture=backgroundPicture,
                };
                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}
