using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.RealTime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications.UserBillNotification;
using Yootek.Notifications;
using Yootek.Services.Dto;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Payment;
using Microsoft.EntityFrameworkCore;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Common.DataResult;

namespace Yootek.Services
{
    public interface IUserBillPaymentAppService : IApplicationService
    {
        Task HandlePaymentForThirdParty(HandPaymentForThirdPartyInput input);
        Task<UserBillPayment> RequestUserBillPayment(CreatePaymentDto input);
    }

    public class UserBillPaymentAppService : YootekAppServiceBase, IUserBillPaymentAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<BillEmailHistory, long> _billEmailRepos;
        private readonly IRepository<Booking, long> _bookingRepos;
        private readonly IRepository<BillConfig, long> _billConfigRepo;
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<Citizen, long> _citizenRepo;
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly BillUtilAppService _billUtilAppService;
        private readonly IUserBillRealtimeNotifier _userBillRealtimeNotifier;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IBillEmailUtil _billEmailUtil;
        private readonly IAppNotifier _appNotifier;
        private readonly HandlePaymentUtilAppService _handlePaymentUtilAppService;

        public UserBillPaymentAppService(
            IBillEmailUtil billEmailUtil,
            IRepository<UserBill, long> userBillRepo,
            IRepository<Booking, long> bookingRepo,
            IRepository<BillConfig, long> billConfigRepo,
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<Citizen, long> citizenRepo,
            IRepository<BillDebt, long> billDebtRepo,
            IRepository<BillEmailHistory, long> billEmailRepos,
            BillUtilAppService billUtilAppService,
            IUserBillRealtimeNotifier userBillRealtimeNotifier,
            IOnlineClientManager onlineClientManager,
            IAppNotifier appNotifier,
            HandlePaymentUtilAppService handlePaymentUtilAppService
        )
        {
            _billEmailUtil = billEmailUtil;
            _userBillRepo = userBillRepo;
            _bookingRepos = bookingRepo;
            _billConfigRepo = billConfigRepo;
            _userBillPaymentRepo = userBillPaymentRepo;
            _citizenRepo = citizenRepo;
            _billUtilAppService = billUtilAppService;
            _userBillRealtimeNotifier = userBillRealtimeNotifier;
            _onlineClientManager = onlineClientManager;
            _appNotifier = appNotifier;
            _billDebtRepo = billDebtRepo;
            _billEmailRepos = billEmailRepos;
            _handlePaymentUtilAppService = handlePaymentUtilAppService;
        }

        [RemoteService(false)]
        public async Task<UserBillPayment> RequestUserBillPayment(CreatePaymentDto request)
        {
            var input = JsonConvert.DeserializeObject<PayMonthlyUserBillsInput>(request.TransactionProperties);
            input.Status = UserBillPaymentStatus.Pending;
            var payment = await _handlePaymentUtilAppService.PayMonthlyUserBillByApartment(input);
            return payment;
        }

        public async Task HandlePaymentForThirdParty(HandPaymentForThirdPartyInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    var userBillPayment = await _userBillPaymentRepo.FirstOrDefaultAsync(input.PaymentId);
                    switch (input.Status)
                    {
                        case EPrepaymentStatus.SUCCESS:
                            await _handlePaymentUtilAppService.UpdatePaymentSuccess(userBillPayment);
                            break;
                        case EPrepaymentStatus.FAILED:
                            await _handlePaymentUtilAppService.CancelPaymentUserBill(userBillPayment);

                            break;
                        default:
                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
