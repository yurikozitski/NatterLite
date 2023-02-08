using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace NatterLite.Services
{
    public class ImageValidator: IImageValidator
    {
        public bool IsImageValid(IFormFile picture)
        {
            FileInfo fileInfo = new FileInfo(picture.FileName);
            string fileExtension = fileInfo.Extension;
            bool IsFileExtensionValid = fileExtension == ".jpg" || fileExtension == ".jpeg";
            if (picture.Length > 2097152 || !IsFileExtensionValid)
                return false;
            return true;
        }
    }
}
