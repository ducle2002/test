using AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services.Dto

{
    [AutoMap(typeof(Images))]
    public class ImageDto : Images
    {

    }
}
