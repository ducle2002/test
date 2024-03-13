using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Amazon.Runtime.Internal.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek;
using Yootek.EntityDb;
using YOOTEK.EntityDb;
using YOOTEK.EntityDb.IMAX.DichVu.DigitalServices;

namespace YOOTEK.Yootek.Services
{
    public class DigitalServicePaymentUtil : YootekAppServiceBase
    {

        private readonly IRepository<DigitalServicePayment, long> _digitalServicePaymentRepository;
        private readonly IRepository<DigitalServiceOrder, long> _digitalServiceOrderRepository;
        

        public DigitalServicePaymentUtil(
             IRepository<DigitalServicePayment, long> digitalServicePaymentRepository,
             IRepository<DigitalServiceOrder, long> digitalServiceOrderRepository
          
            )
        {
            _digitalServicePaymentRepository = digitalServicePaymentRepository;
            _digitalServiceOrderRepository = digitalServiceOrderRepository;
          
        }

        public async Task<DigitalServicePayment> HandlePaymentSuccess(long orderId, decimal paymentAmount, DigitalServicePaymentMethod method, string note, string properties)
        {
            try
            {
                var  order = await _digitalServiceOrderRepository.FirstOrDefaultAsync(x => x.Id == orderId 
                && (x.PaymentState == DigitalServicePaymentState.Pending || x.PaymentState == DigitalServicePaymentState.Debt));
                if (order == null) throw new UserFriendlyException("Data not found");

                var payment = new DigitalServicePayment()
                {
                    Amount = paymentAmount,
                    ApartmentCode = order.ApartmentCode,
                    BuildingId = order.BuildingId,
                    Method = method,
                    OrderId = orderId,
                    TenantId = order.TenantId,
                    UrbanId = order.UrbanId,
                    ServiceId = order.ServiceId,
                    Status = DigitalServicePaymentStatus.PENDING,
                    Note = note,
                    Properties = properties
                };

                var amountMustPay = order.PaymentState == DigitalServicePaymentState.Pending ? order.TotalAmount : order.TotalDebtOrBalance;
                if (amountMustPay == paymentAmount)
                {
                    order.PaymentState = DigitalServicePaymentState.Paid;
                    order.TotalDebtOrBalance = 0;
                }
                else if(amountMustPay > paymentAmount)
                {
                    order.PaymentState = DigitalServicePaymentState.Debt;
                    order.TotalDebtOrBalance = amountMustPay - paymentAmount;
                }
                else
                {
                    order.PaymentState = DigitalServicePaymentState.Balance;
                    order.TotalDebtOrBalance = paymentAmount - amountMustPay;
                }

                await _digitalServicePaymentRepository.InsertAndGetIdAsync(payment);
                await _digitalServiceOrderRepository.UpdateAsync(order);

                return payment;

            }catch( Exception ex )
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}
