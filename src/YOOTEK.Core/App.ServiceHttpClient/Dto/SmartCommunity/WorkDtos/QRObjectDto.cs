using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity
{
    public enum QRCodeType
    {
        Text = 0,
        Url = 1,
        Image = 2,
        Video = 3,
        Audio = 4,
        File = 5,
    }

    public enum QRCodeStatus
    {
        Active = 0,
        Inactive = 1,
    }

    public enum QRCodeActionType
    {
        None = 0,
        CarParking = 1,
        Work = 2,
        Asset = 3,
        Meter = 4,
    }

    public class QRObjectDto : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Code { get; set; }
        [CanBeNull] public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        public QRCodeType Type { get; set; }
        public QRCodeStatus Status { get; set; }
        public QRCodeActionType ActionType { get; set; }
        public string Data { get; set; }
    }

    public class GetListQRObjectDto : CommonInputDto
    {
        public QRCodeStatus? Status { get; set; }
        public QRCodeActionType? ActionType { get; set; }
        public QRCodeType? Type { get; set; }
    }

    public class GetListQRObjectByListCodeInput
    {
        [CanBeNull] public List<string> ListCode { get; set; }
        public QRCodeActionType? ActionType { get; set; }
    }

    public class GetInformationByQRCodeInput
    {
        public string Code { get; set; }
        public QRCodeActionType ActionType { get; set; }
    }

    public class CreateQRObjectDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        public QRCodeType Type { get; set; }
        public QRCodeStatus Status { get; set; }
        public QRCodeActionType ActionType { get; set; }
        public string Data { get; set; }
    }

    public class GetQRObjectByCodeDto
    {
        public string Code { get; set; }
    }

    public class UpdateQRObjectDto
    {
        public long Id { get; set; }
        [CanBeNull] public string Code { get; set; }
        [CanBeNull] public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        public QRCodeType? Type { get; set; }
        public QRCodeStatus? Status { get; set; }
        public QRCodeActionType? ActionType { get; set; }
        [CanBeNull] public string Data { get; set; }
    }

    public class DeleteQRObjectDto
    {
        public long Id { get; set; }
        public string Code { get; set; }
    }
}