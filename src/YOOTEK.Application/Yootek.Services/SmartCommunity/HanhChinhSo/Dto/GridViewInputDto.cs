
using Yootek.Common;
using System;

namespace Yootek.Services
{
    //public class GetConfigAdministrativeGridViewInputDto
    //{
    //    public long? Id { get; set; }
    //    public long? OrganizationUnitId { get; set; }
    //    public long TypeId { get; set; }
    //}

    public class GetAdministrativePropertyInputDto
    {
        public long? Id { get; set; }
        public long? ConfigId { get; set; }
        public long? TypeId { get; set; }
    }


    public class GetPropertyAdministrativeGridViewInputDto
    {
        public long? Id { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long ADTypeId { get; set; }
        public long ConfigId { get; set; }
    }

    public class GetAdministratvieGridViewInputDto : CommonInputDto
    {
        public long? Id { get; set; }
        public FormGetAD FormId { get; set; }
        public FormTimeSelect? TimeSelect { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long ADTypeId { get; set; }
        public int? State { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public string KeySearch { get; set; }
    }

    public enum FormGetAD
    {
        UserGetAll = 1,
        UserGetRequesting = 11,
        UserGetAccepted = 12,
        UserGetDenied = 13,
        AdminGetAll = 2,
        AdminGetRequesting = 21,
        AdminGetAccepted = 22,
        AdminGetDenied = 23,

    }

    public class GetAllAdministrativeByKeySearch : CommonInputDto
    {
        public long ADTypeId { get; set; }
        public string KeySearch { get; set; }
    }

}
