﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public class SendUserBillNotificationInput
    {
        public string ApartmentCode { get; set; }
        public DateTime Period { get; set; }
    }
}