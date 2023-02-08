using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NatterLite.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Name should have from 3 to 20 symbols")]
        [RegularExpression("^[A-Za-z'-]{1,}", ErrorMessage = "Only letters and ' - are allowed")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Name should have from 3 to 20 symbols")]
        [RegularExpression("^[A-Za-z'-]{1,}", ErrorMessage = "Only letters and ' - are allowed")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Unique name is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Name should have from 3 to 20 symbols")]
        [RegularExpression("^@[A-Za-z0-9'-_]{1,}", ErrorMessage = "Only letters,numbers and '-_ are allowed, @ should be first")]
        [Remote(action: "CheckUniqueName", controller: "DataCheck", ErrorMessage = "This unique name is already in use")]
        public string UniqueName { get; set; }
        public string Country { get; set; }
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        [Remote(action: "CheckEmail", controller: "DataCheck", ErrorMessage = "Email is already in use")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password should have from 8 to 50 symbols")]
        [RegularExpression("^[A-Za-z0-9'-_]{1,}", ErrorMessage = "Only letters,numbers and '-_ are allowed")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords don't match")]
        public string ConfirmPassword { get; set; }

        //[Remote(action: "CheckImage", controller: "DataCheck", ErrorMessage = "Picture size bigger than 2Mb or has invalid extension")]
        public IFormFile ProfilePicture { get; set; }

        //[Remote(action: "CheckImage", controller: "DataCheck", ErrorMessage = "Picture size bigger than 2Mb or has invalid extension")]
        public IFormFile BackgroundPicture { get; set; }
    }
}
