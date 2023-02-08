using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NatterLite.Services
{
    public interface IPicturesProvider
    {
        public byte[] GetDefaultPicture(string imageLocation);
    }
}
