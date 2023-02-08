using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace NatterLite.Services
{
    public class DefaultUserPicturesProvider: IPicturesProvider
    {
        public byte[] GetDefaultPicture(string imageLocation)
        {
            byte[] imageData = null;
            FileInfo fileInfo = new FileInfo(imageLocation);
            long imageFileLength = fileInfo.Length;
            FileStream fs = new FileStream(imageLocation, FileMode.Open, FileAccess.Read);
            using (BinaryReader br = new BinaryReader(fs))
            {
                imageData = br.ReadBytes((int)imageFileLength);
            }
            return imageData;
        }
    }
}
