using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Yootek.Organizations
{
    public class SchedulerNotification: Entity<long>
    {
        public string Message { get; set; }
        public string Users { get; set; }
        public bool IsSocial { get; set; }
        public bool IsOnlyFirebase { get; set; }
        [StringLength(1000)]
        public string Header {  get; set; }
        [StringLength(2000)]
        public string Content {  get; set; }
        public bool IsScheduler {  get; set; }
        public bool IsCompleted { get; set; }
        public NotificationType TypeNotification { get; set; }
        public TypeScheduler TypeScheduler { get; set; }
        public DateTime? Time { get; set; }
        public DateTime? DueDate { get; set; }
        public List<DateTime> ListTimes {  get; set; }
    }

    public enum TypeScheduler
    {
        Normal = 1,
        LoopDay = 2,
        LoopMonth = 3,
        LoopYear = 4,
      
    }


    public enum NotificationType
    {
        [EnumValueString(AppRouterLinks.AppUser_DigitalNotification)]
        AppUser_DigitalNotification = 101,

        [EnumValueString(AppRouterLinks.AppUser_Vote)]
        AppUser_Vote = 102,

        [EnumValueString(AppRouterLinks.AppUser_CitizenFeedback)]
        AppUser_CitizenFeedback = 103,

        [EnumValueString(AppRouterLinks.AppUser_Invoice)]
        AppUser_Invoice = 104,

        [EnumValueString(AppRouterLinks.AppUser_DigitalService)]
        AppUser_DigitalService = 105,

        [EnumValueString(AppRouterLinks.AppUser_LocalAmenities)]
        AppUser_LocalAmenities = 106,
        [EnumValueString(AppRouterLinks.AppUser_Administrative)]
        AppUser_Administrative = 107,
        [EnumValueString(AppRouterLinks.AppUser_QnA)]
        AppUser_QnA = 108,
        [EnumValueString(AppRouterLinks.AppUser_Hotline)]
        AppUser_Hotline = 109,
        [EnumValueString(AppRouterLinks.AppUser_Social_Shopping)]
        AppUser_Social_Shopping = 110,
        [EnumValueString(AppRouterLinks.AppUser_Social_Booking)]
        AppUser_Social_Booking = 111,
        [EnumValueString(AppRouterLinks.AppUser_Social_Map)]
        AppUser_Social_Map = 112,
        [EnumValueString(AppRouterLinks.AppUser_Social_FlashSale)]
        AppUser_Social_FlashSale = 113,
        [EnumValueString(AppRouterLinks.AppUser_Social_Voucher)]
        AppUser_Social_Voucher = 114,
        [EnumValueString(AppRouterLinks.AppUser_Social_TopProvider)]
        AppUser_Social_TopProvider = 115,
        [EnumValueString(AppRouterLinks.AppUser_Social_TopProduct)]
        AppUser_Social_TopProduct = 116,
        [EnumValueString(AppRouterLinks.AppUser_Social_TopService)]
        AppUser_Social_TopService = 117,
        [EnumValueString(AppRouterLinks.AppUser_Social_Education)]
        AppUser_Social_Education = 118,
        [EnumValueString(AppRouterLinks.AppUser_Social_Cuisine)]
        AppUser_Social_Cuisine = 119,
        [EnumValueString(AppRouterLinks.AppUser_Social_Medical)]
        AppUser_Social_Medical = 120,
        [EnumValueString(AppRouterLinks.AppUser_Social_HealthCare)]
        AppUser_Social_HealthCare = 121,
        [EnumValueString(AppRouterLinks.AppUser_Social_Travel)]
        AppUser_Social_Travel = 122,
        [EnumValueString(AppRouterLinks.AppUser_Social_Stay)]
        AppUser_Social_Stay = 123,
        [EnumValueString(AppRouterLinks.AppUser_Social_Village)]
        AppUser_Social_Village = 124,
        [EnumValueString(AppRouterLinks.AppUser_Social_Entertainment)]
        AppUser_Social_Entertainment = 125,
        [EnumValueString(AppRouterLinks.AppUser_Social_Repair)]
        AppUser_Social_Repair = 126,
        [EnumValueString(AppRouterLinks.AppUser_Social_Job)]
        AppUser_Social_Job = 127,
        [EnumValueString(AppRouterLinks.AppUser_Social_Traffic)]
        AppUser_Social_Traffic = 128,
        [EnumValueString(AppRouterLinks.AppUser_Social_Sport)]
        AppUser_Social_Sport = 129,

        [EnumValueString(AppRouterLinks.AppSeller_ShopInfo)]
        AppSeller_ShopInfo = 201,
        [EnumValueString(AppRouterLinks.AppSeller_ProductInfo)]
        AppSeller_ProductInfo = 202,
        [EnumValueString(AppRouterLinks.AppSeller_Amenities)]
        AppSeller_Amenities = 203,
        [EnumValueString(AppRouterLinks.AppSeller_Voucher)]
        AppSeller_Voucher = 204,
        [EnumValueString(AppRouterLinks.AppSeller_Promotion)]
        AppSeller_Promotion = 205,
        [EnumValueString(AppRouterLinks.AppSeller_Ads)]
        AppSeller_Ads = 206,
        [EnumValueString(AppRouterLinks.AppSeller_FlashSale)]
        AppSeller_FlashSale = 207,

    }

    public static class AppRouterLinks
    {
        public const string AppUser_DigitalNotification = "yoolife://app/notification";
        public const string AppUser_Vote = "yoolife://app/evote";
        public const string AppUser_CitizenFeedback = "yoolife://app/citizenfeedback";
        public const string AppUser_Invoice = "yoolife://app/invoice";
        public const string AppUser_DigitalService = "yoolife://app/digitalservice";
        public const string AppUser_LocalAmenities = "yoolife://app/localamenities";
        public const string AppUser_Administrative = "yoolife://app/administrative";
        public const string AppUser_QnA = "yoolife://app/qna";
        public const string AppUser_Hotline = "yoolife://app/hotline";
        public const string AppUser_ChatUser = "yoolife://app/chat-user";
        public const string AppUser_ChatOrganization = "yoolife://app/chat-organization";
        public const string AppUser_ChatSeller = "yoolife://app/chat-seller";

        public const string AppUser_Social_Shopping = "yoolife://app/smartsocial-shopping";
        public const string AppUser_Social_Booking = "yoolife://app/smartsocial-booking";
        public const string AppUser_Social_Map = "yoolife://app/smartsocial-map";

        public const string AppUser_Social_FlashSale = "yoolife://app/smartsocial-flashsale";
        public const string AppUser_Social_Voucher = "yoolife://app/smartsocial-voucher";

        public const string AppUser_Social_TopProvider = "yoolife://app/smartsocial-topprovider";
        public const string AppUser_Social_TopProduct = "yoolife://app/smartsocial-topproduct";
        public const string AppUser_Social_TopService = "yoolife://app/smartsocial-topservice";

        public const string AppUser_Social_Education = "yoolife://app/smartsocial-eduction";
        public const string AppUser_Social_Cuisine = "yoolife://app/smartsocial-cuisine";
        public const string AppUser_Social_Medical = "yoolife://app/smartsocial-medical";
        public const string AppUser_Social_HealthCare = "yoolife://app/smartsocial-healthcare";
        public const string AppUser_Social_Travel = "yoolife://app/smartsocial-travel";
        public const string AppUser_Social_Stay = "yoolife://app/smartsocial-stay";
        public const string AppUser_Social_Village = "yoolife://app/smartsocial-village";
        public const string AppUser_Social_Entertainment = "yoolife://app/smartsocial-entertainment";

        public const string AppUser_Social_Repair = "yoolife://app/smartsocial-repair";
        public const string AppUser_Social_Job = "yoolife://app/smartsocial-job";
        public const string AppUser_Social_Traffic = "yoolife://app/smartsocial-traffic";
        public const string AppUser_Social_Sport = "yoolife://app/smartsocial-sport";

        public const string AppSeller_ShopInfo = "yooseller://shopinfo";
        public const string AppSeller_ProductInfo = "yooseller://productinfo";
        public const string AppSeller_Amenities = "yooseller://amenities";

        public const string AppSeller_Voucher = "yooseller://voucher";
        public const string AppSeller_Promotion = "yooseller://promotion";
        public const string AppSeller_Ads = "yooseller://ads";
        public const string AppSeller_FlashSale = "yooseller://flashsale";
        public const string AppSeller_ChatUser = "yooseller://chat-user/chatbox";


        public static string GetEnumValue(Enum en)
        {
            Type type = en.GetType();

            try
            {
                MemberInfo[] memInfo = type.GetMember(en.ToString());

                if (memInfo != null && memInfo.Length > 0)
                {
                    object[] attrs = memInfo[0].GetCustomAttributes(typeof(EnumValueString), false);

                    if (attrs != null && attrs.Length > 0)
                        return ((EnumValueString)attrs[0]).Value;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return en.ToString();
        }

    }

    public class EnumValueString : Attribute
    {
        public string Value;

        public EnumValueString(string text)
        {
            this.Value = text;
        }

    }

}
