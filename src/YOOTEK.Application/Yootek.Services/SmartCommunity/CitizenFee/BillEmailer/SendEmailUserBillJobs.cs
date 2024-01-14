using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Notifications;
using Yootek.Services.BillEmailer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    internal class SendEmailUserBillJobs : AsyncBackgroundJob<SendEmailUserBillJobArgs>, ITransientDependency
    {
        private readonly IIocResolver _iocResolver;
        private readonly IBillEmailUtil _userBillEmailUtil;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailUserBillJobArgs"/> class.
        /// </summary>
        public SendEmailUserBillJobs(
            IBillEmailUtil userBillEmailUtil,
            IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
            _userBillEmailUtil = userBillEmailUtil;
        }


        public async override Task ExecuteAsync(SendEmailUserBillJobArgs args)
        {
            foreach (var code in args.ApartmentCodes)
            {
                try
                {
                    await _userBillEmailUtil.SendEmailToApartmentAsync(code, args.Period, args.TenantId);
                }
                catch
                {

                }
            }
        }
    }
}
