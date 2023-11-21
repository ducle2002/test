using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.EntityFrameworkCore.Repositories
{
    public class UserBillPaymentRepository : IMAXRepositoryBase<UserBillPayment, long>, IRepository<UserBillPayment, long>
    {
        public UserBillPaymentRepository(IDbContextProvider<IMAXDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public override Task<long> InsertAndGetIdAsync(UserBillPayment entity)
        {
            var id = base.InsertAndGetIdAsync(entity);
            entity.PaymentCode = "PM-" + id + "-" + GetUniqueKey(6);
            UnitOfWorkManager.Current.SaveChangesAsync();
            return id;
        }

        public override long InsertAndGetId(UserBillPayment entity)
        {
            var id = base.InsertAndGetId(entity);
            entity.PaymentCode = "PM-" + id + "-" + GetUniqueKey(6);
            UnitOfWorkManager.Current.SaveChanges();
            return id;
        }

    }
}
