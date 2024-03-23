using Abp.Application.Services;
using Abp.Auditing;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Abp.UI;
using Google.Protobuf.WellKnownTypes;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity;
using Yootek.Authorization.Users;
using Yootek.MultiTenancy;
using Yootek.Services;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common.Enum;
using YOOTEK.EntityDb;

namespace Yootek
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    [DisableAuditing]
    public abstract class YootekAppServiceBase : ApplicationService
    {
        public static Benchmark mb = new Benchmark();
        private static ConcurrentDictionary<long, List<CityNotificationDto>> UserCityNotifications = new ConcurrentDictionary<long, List<CityNotificationDto>>();
        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }

        protected YootekAppServiceBase()
        {
            LocalizationSourceName = YootekConsts.LocalizationSourceName;
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            return user;
        }

        protected virtual Task<Tenant> GetCurrentTenantAsync()
        {
            return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }


        protected virtual string GetUniqueKey(int maxSize = 10)
        {
            char[] chars = new char[36];
            string a;
            a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }

        protected virtual bool IsPropertyExist(dynamic settings, string name)
        {
            if (settings is ExpandoObject)
                return ((IDictionary<string, object>)settings).ContainsKey(name);

            return settings.GetType().GetProperty(name) != null;
        }


        protected string QRCodeGen(long id, QRCodeActionType type)
        {
            return $"{type}-{AbpSession.TenantId}-{id}-{GetUniqueKey(8)}";
        }

        #region Yootek.Business.Smart-social
        protected static string ConvertDatetimeToString(DateTime datetime)
        {
            return datetime.ToString("dd/MM/yyyy HH:mm/ss");
        }
        protected static Timestamp ConvertDatetimeToTimestamp(DateTime dateTime)
        {
            DateTime date = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return Timestamp.FromDateTime(date);
        }
        protected static string FormatDateTime(DateTime datetime, string? dateTimeFormat = "dd/MM/yyyy")
        {
            return datetime.ToString(dateTimeFormat);
        }
        #endregion

        #region user bill
        protected static string FormatCost(double? value) => value.HasValue && value > 0 ? string.Format("{0:#,#.##}", value.Value) : "";
        protected static string FormatCost(long? value) => value.HasValue && value > 0 ? string.Format("{0:#,#.##}", value.Value) : "";
        protected static string FormatCost(int? value) => value.HasValue && value > 0 ? string.Format("{0:#,#.##}", value.Value) : "";
        protected static string FormatCost(decimal? value) => value.HasValue && value > 0 ? string.Format("{0:#,#.##}", value.Value) : "";
        protected static string GetStringValue(object value) => value?.ToString() ?? "";
        protected static string FormatNumberToTwoDigits(int number)
        {
            if (number < 10)
            {
                return number.ToString("D2");
            }
            else
            {
                return number.ToString();
            }
        }
        #endregion

        //generate CitizenCode
        protected string GetUniqueCitizenCode()
        {
            long maxSize = 6;
            //long minSize = 3;
            char[] chars = new char[62];
            string x;
            x = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            chars = x.ToCharArray();
            long size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            //size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder((int)size);
            foreach (byte y in data)
            {
                result.Append(chars[y % (chars.Length - 1)]);

                //return result.ToString();
            }

            return result.ToString();
            //Console.WriteLine(result);
        }

        // System.Guid guid = System.Guid.NewGuid();

        #region Excel
        protected T GetCellValue<T>(ExcelWorksheet worksheet, int row, int columnIndex)
        {
            try
            {
                var cellValue = worksheet.Cells[row, columnIndex].Value?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(cellValue))
                {
                    return default; // Return the default value for the data type
                }
                return (T)Convert.ChangeType(cellValue, typeof(T));
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException($"Error at row {row}: {worksheet.Cells[row, columnIndex].Address} is invalid.");
            }
        }
        protected T GetCellValueNotDefault<T>(ExcelWorksheet worksheet, int row, int columnIndex)
        {
            try
            {
                var cellValue = worksheet.Cells[row, columnIndex].Value?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(cellValue))
                {
                    throw new UserFriendlyException($"Error at row {row}: {worksheet.Cells[row, columnIndex].Address} is required.");
                }
                return (T)Convert.ChangeType(cellValue, typeof(T));
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException($"Error at row {row}: {worksheet.Cells[row, columnIndex].Address} is invalid.");
            }
        }
        protected static bool IsFileExtensionSupported(string fileExt)
        {
            string[] supportedExtensions = new[] { ".xlsx", ".xls" };
            return supportedExtensions.Contains(fileExt);
        }
        protected Result<T> GetCellValueExcelCheck<T>(ExcelWorksheet worksheet, int row, int columnIndex, bool required = false)
        {
            try
            {
                var cellValue = worksheet.Cells[row, columnIndex].Value?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(cellValue))
                {
                    if (required)
                        return new Result<T> { Error = true, Message = $"Error at row {row}: {worksheet.Cells[1, columnIndex].Value?.ToString()?.Trim()} is required." };
                    else
                        return new Result<T> { Value = default };
                }
                return new Result<T> { Value = (T)Convert.ChangeType(cellValue, typeof(T)) };

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                return new Result<T> { Error = true, Message = $"Error at row {row}: {worksheet.Cells[row, columnIndex].Address} is invalid." };
            }
        }

        public class Result<T>
        {
            public T Value { get; set; }
            public bool Error { get; set; }
            public string Message { get; set; }
        }

        public class ResultImportExcel
        {
            public List<string> Errors { get; set; }
            public int TotalSuccess { get; set; }
            public int TotalReplace { get; set; }
        }
        #endregion

        #region number convert
        protected decimal DecimalRouding(object value, int number = 0) => value == null ? 0 : Math.Round((decimal)value, number);
       // protected decimal DecimalRoudingUp(object value) => value == null ? 0 : Math.Round((decimal)value + (decimal)0.5, 0);
        protected decimal DecimalRoudingUp(double value) => Math.Round((decimal)value + (decimal)0.5, 0);
        protected decimal DecimalRoudingUp(decimal value) => Math.Round(value + (decimal)0.5, 0);
        #endregion

        protected UserBillPaymentMethod CheckMethod(UserBillPaymentMethod method)
        {
            if (method == UserBillPaymentMethod.OnePay1 ||
                method == UserBillPaymentMethod.OnePay2 ||
                method == UserBillPaymentMethod.OnePay3) return UserBillPaymentMethod.OnePay;
            return method;
        }

        protected EPaymentMethod CheckMethod(EPaymentMethod method)
        {
            if (method == EPaymentMethod.OnePay1 ||
                method == EPaymentMethod.OnePay2 ||
                method == EPaymentMethod.OnePay3) return EPaymentMethod.Onepay;
            return method;
        }
    }
}
