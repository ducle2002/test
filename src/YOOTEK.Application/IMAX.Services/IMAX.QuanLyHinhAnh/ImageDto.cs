using AutoMapper;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services.Dto

{
    [AutoMap(typeof(Images))]
    public class ImageDto : Images
    {

    }
}
