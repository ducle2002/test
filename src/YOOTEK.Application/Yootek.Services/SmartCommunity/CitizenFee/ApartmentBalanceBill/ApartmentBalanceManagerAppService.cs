using Abp.Application.Services;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Yootek;
using Yootek.Authorization.Users;
using Yootek.EntityDb;
using YOOTEK.EntityDb;

namespace YOOTEK.Yootek.Services.SmartCommunity.CitizenFee.ApartmentBalanceBill
{
    public interface IApartmentBalanceManagerAppService : IApplicationService
    {

    }

    public class ApartmentBalanceManagerAppService : YootekAppServiceBase, IApartmentBalanceManagerAppService
    {
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly IRepository<UserBillPaymentHistory, long> _billPaymentHistoryRepos;
        private readonly IRepository<ApartmentBalance, long> _apartmentBalanceRepos;


        public ApartmentBalanceManagerAppService(
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<User, long> userRepos, IRepository<UserBill, long> userBillRepo,
            IRepository<BillDebt, long> billDebtRepo,
            IRepository<CitizenTemp, long> citizenTempRepos,
            IRepository<UserBillPaymentHistory, long> billPaymentHistoryRepos,
            IRepository<ApartmentBalance, long> apartmentBalanceRepos
            )

        {
            _userBillPaymentRepo = userBillPaymentRepo;
            _userRepos = userRepos;
            _userBillRepo = userBillRepo;
            _citizenTempRepos = citizenTempRepos;
            _billDebtRepo = billDebtRepo;
            _billPaymentHistoryRepos = billPaymentHistoryRepos;
            _apartmentBalanceRepos = apartmentBalanceRepos;
        }



    }
}
