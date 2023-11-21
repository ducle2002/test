

using System.Collections.Generic;

namespace IMAX.Authorization
{
    public static class PermissionConst
    {
        public enum HONG_BANG_ROLE
        {
            ResourceEnvironment = 1,
            FinancePlanning = 2,
            Internal = 3,
            Inspection = 4,
            Judical = 5,
            Commerce = 6,
            Invalids = 7,
            Education = 8,
            CulturalInfomation = 9,
            Zoning = 10,
            DistrictDepartment = 11,
            Police = 12,
            Army = 13

        }

        public static Dictionary<int, string> HongBangRoleMap = new Dictionary<int, string>();

        //public static void MapInit()
        //{
        //    if(HongBangRoleMap.Count < 13)
        //    {
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.ResourceEnvironment, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_ResourceEnvironment);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.FinancePlanning, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_FinancePlanning);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.Internal, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_Internal);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.Inspection, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_Inspection);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.Judical, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_Judical);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.Commerce, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_Commerce);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.Invalids, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_Invalids);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.Education, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_Education);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.CulturalInfomation, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_CulturalInfomation);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.Zoning, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_Zoning);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.DistrictDepartment, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_DistrictDepartment);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.Police, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_Police);
        //        HongBangRoleMap.Add((int)HONG_BANG_ROLE.Army, PermissionNames.Pages_SmartCommunity_Citizens_Reflects_Army);

        //    }
        //}
    }
}
