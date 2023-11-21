using Abp.Application.Services;
using Abp.Domain.Repositories;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using IMAX.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Abp;
using Abp.Authorization;
using IMAX.Notifications;
using Abp.RealTime;

namespace IMAX.Services
{

    public class BillUtilAppService : IMAXAppServiceBase
    {
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<BillConfig, long> _billConfigRepo;
        private readonly IRepository<Citizen, long> _citizenRepo;
        private readonly IRepository<HomeMember, long> _homeMemberRepo;

        private readonly IOnlineClientManager _onlineClientManager;

        private readonly IAppNotifier _appNotifier;
        // private readonly IUserBillRealtimeNotifier _userBillRealtimeNotifier;

        public BillUtilAppService(
            IRepository<UserBill, long> userBillRepo,
            IRepository<BillConfig, long> billConfigRepo,
            IRepository<Citizen, long> citizenRepo,
            IRepository<HomeMember, long> HomeMemberRepo,
            //  IUserBillRealtimeNotifier userBillRealtimeNotifier,
            IAppNotifier appNotifier,
            IOnlineClientManager onlineClientManager
        )
        {
            _userBillRepo = userBillRepo;
            _billConfigRepo = billConfigRepo;

            _citizenRepo = citizenRepo;
            _homeMemberRepo = HomeMemberRepo;
            //  _userBillRealtimeNotifier = userBillRealtimeNotifier;
            _appNotifier = appNotifier;
            _onlineClientManager = onlineClientManager;
        }

        private double CalculateDependOnLevel(PriceDto[] levels, double amount)
        {
            string[] keys = { "start" };
            Array.Sort(keys, levels);

            var levelIndex = 0;
            for (var i = 0; i < levels.Count(); i++)
            {
                if (amount >= levels[i].From)
                {
                    levelIndex = i;
                }
            }

            var result = 0.0;
            for (var i = 0; i <= levelIndex; i++)
            {
                var level = levels[i];
                if (i != 0) level.From = level.From - 1;

                if (amount < level.To)
                {
                    result += (double)(level.Value * (amount - level.From));
                    break;
                }

                if (i == levelIndex)
                {
                    result += (double)(level.Value * (amount - level.From));
                    break;
                }

                result += (double)(level.Value * (level.To - level.From));
            }

            return result;
        }

        #region Schedule

        [RemoteService(false)]
        [AbpAllowAnonymous]
        public Task SendUserBillToClient(UserBill[] bills, bool isMembersSend = true, long? userId = null)
        {
            // var costResult = CalculateUserBill(bills).Result;
            // bills.Cost = costResult.Cost;
            // bills.LastCost = costResult.LastCost;
            // bills.Surcharges = costResult.Surcharges;

            if (isMembersSend)
            {
                SendBillMessageToMembers(bills);
            }
            else
            {
                SendBillMessageToUser(bills, userId.Value);
            }

            return Task.CompletedTask;
        }

        protected async Task SendBillMessageToMembers(UserBill[] bills)
        {
            var apartmentCodes = bills.Select(x => x.ApartmentCode).ToList();
            var members = _homeMemberRepo.GetAllList(x => apartmentCodes.Any(y => y == x.ApartmentCode))
                .Select(x => x.UserId)
                .ToList().ToHashSet().ToList();
            if (members == null)
            {
                return;
            }

            var users = new List<UserIdentifier>();
            foreach (var usId in members)
            {
                var us = new UserIdentifier(bills[0].TenantId, usId.Value);
                users.Add(us);
                var clients = ( await _onlineClientManager.GetAllClientsAsync())
                    .Where(c => c.UserId == usId)
                    .ToImmutableList();
                //    _userBillRealtimeNotifier.NotifyUpdateStateBill(clients, item);
            }

            var totalCost = bills.Sum(x => x.LastCost);

            if (totalCost != null)
            {
                string message = GetMessageText("hóa đơn", bills[0].ApartmentCode, (double)totalCost);
               await  _appNotifier.MultiSendMessageAsync("UserBillMessage", users.ToArray(), message);
            }

            return;
        }

        protected async Task SendBillMessageToUser(UserBill[] bills, long userId)
        {
            var user = new UserIdentifier(bills[0].TenantId, userId);
            var clients = (await _onlineClientManager.GetAllClientsAsync())
                .Where(c => bills.Any(b => b.CreatorUserId == userId))
                .ToImmutableList();
            var totalCost = bills.Sum(x => x.LastCost);
            if (totalCost != null)
            {
                string message = GetMessageText("hóa đơn", bills[0].ApartmentCode, (double)totalCost);
                //    _userBillRealtimeNotifier.NotifyUpdateStateBill(clients, item);
               await  _appNotifier.MultiSendMessageAsync("UserBillMessage", new UserIdentifier[] { user }, message);
            }

            return;
        }

        private string GetMessageText(string name, string apartmentCode, double cost)
        {
            return $"Thông báo {name} căn hộ {apartmentCode}: {cost} VND ";
        }

        #endregion
    }
}