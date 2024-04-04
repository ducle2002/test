using Abp.Dependency;
using Abp.IO.Extensions;
using Abp.Reflection.Extensions;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Yootek.Authorization.BillInvoices
{
    public interface IBillInvoiceTemplateProvider
    {
        string GetBillInvoiceTemplate(int? tenantId);
        string GetBillPaymentTemplate(int? tenantId);
    }
    public class BillInvoiceTemplateProvider : IBillInvoiceTemplateProvider, ITransientDependency
    {
        private readonly IConfiguration _configuration;
        public BillInvoiceTemplateProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetBillInvoiceTemplate(int? tenantId)
        {
            if (tenantId == _configuration.GetValue<int>("CustomTenant:HudlandTenantId"))
            {
                using (var stream = typeof(BillInvoiceTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.BillInvoices.Templates.hudland.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            if (tenantId == _configuration.GetValue<int>("CustomTenant:Vina22TenantId"))
            {
                using (var stream = typeof(BillInvoiceTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.BillInvoices.Templates.vina22.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            if (tenantId == 94)
            {
                using (var stream = typeof(BillInvoiceTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.BillInvoices.Templates.vinasinco.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            if (tenantId == 115)
            {
                using (var stream = typeof(BillInvoiceTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.BillInvoices.Templates.lathanh.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else
            {
                using (var stream = typeof(BillInvoiceTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.BillInvoices.Templates.default.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
        }
        public string GetBillPaymentTemplate(int? tenantId)
        {

            if (tenantId == 115)
            {
                using (var stream = typeof(BillInvoiceTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.BillInvoices.Templates.lathanh.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else
            {
                using (var stream = typeof(BillInvoiceTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.BillInvoices.Templates.default.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }


        }
    }
}
