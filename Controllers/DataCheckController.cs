using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using NatterLite.Models;
using Microsoft.EntityFrameworkCore;

namespace NatterLite.Controllers
{
    public class DataCheckController : Controller
    {
        private readonly ApplicationContext db;
        public DataCheckController(ApplicationContext context)
        {
            db = context;
        }
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
                return Json(false);
            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckEmail_Edit(string email)
        {
            User currentuser = await db.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            User user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user!=null&&currentuser.Id == user.Id) return Json(true);
            if (user != null)
                return Json(false);
            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckUniqueName(string uniqueName)
        {
            User user = await db.Users.FirstOrDefaultAsync(u=>u.UserName==uniqueName);
            if (user!=null)
                return Json(false);
            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckUniqueName_Edit(string uniqueName)
        {
            User currentuser = await db.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            User user = await db.Users.FirstOrDefaultAsync(u => u.UserName == uniqueName);
            if (user != null && currentuser.Id == user.Id) return Json(true);
            if (user != null)
                return Json(false);
            return Json(true);
        }
    }
}
