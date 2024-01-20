//using Abp.BackgroundJobs;
//using Abp.Dependency;
//using Abp.Domain.Repositories;
//using Abp.Domain.Uow;
//using Abp.Notifications;
//using Yootek.EntityDb;
//using Yootek.Services.BillEmailer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Yootek.Services
//{
//    internal class UserBillBackGroundJobs : IAsyncBackgroundJob<List<UserBill>>, ITransientDependency
//    {
//        private readonly IIocResolver _iocResolver;
//        private readonly IBillEmailUtil _userBillEmailUtil;
//        private readonly IRepository<UserBill, long> _userBillRepository;
//        private readonly IUnitOfWorkManager _unitOfWorkManager;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="List<UserBill>"/> class.
//        /// </summary>
//        public UserBillBackGroundJobs(
//            IBillEmailUtil userBillEmailUtil,
//            IRepository<UserBill, long> userBillRepository,
//            IUnitOfWorkManager unitOfWorkManager,
//            IIocResolver iocResolver)
//        {
//            _iocResolver = iocResolver;
//            _userBillEmailUtil = userBillEmailUtil;
//            _userBillRepository = userBillRepository;
//            _unitOfWorkManager =  unitOfWorkManager;
//        }

//        public async Task ExecuteAsync(List<UserBill> listBills)
//        {
//            foreach (UserBill userBill in listBills)
//            {
//                var id = await _userBillRepository.InsertAndGetIdAsync(userBill);
//                userBill.Code = "HD" + userBill.Id + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month) + "" + DateTime.Now.Year;
//                await _unitOfWorkManager.Current.SaveChangesAsync();
//            }
           
//        }
//    }
//}
