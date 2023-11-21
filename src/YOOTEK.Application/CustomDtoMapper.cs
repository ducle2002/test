using AutoMapper;
using IMAX.Authorization.Permissions.Dto;
using IMAX.EntityDb;
using IMAX.IMAX.EntityDb.SmartCommunity.Apartment;
using IMAX.IMAX.EntityDb.SmartCommunity.Phidichvu;
using IMAX.IMAX.Services.IMAX.SmartCommunity.Building.Dto;
using IMAX.IMAX.Services.IMAX.SmartCommunity.CitizenFee.Dto;
using IMAX.Organizations;
using IMAX.Services;
using IMAX.Services.Dto;
using System.Collections;
using System.Collections.Generic;
using Permission = Abp.Authorization.Permission;

namespace IMAX
{
    internal static class CustomDtoMapper
    {
        private static volatile bool _mappedBefore;
        private static readonly object SyncObj = new object();

        public static void CreateMappings(IMapperConfigurationExpression mapper)
        {
            lock (SyncObj)
            {
                if (_mappedBefore)
                {
                    return;
                }

                CreateMappingsInternal(mapper);

                _mappedBefore = true;
            }
        }

        private static void CreateMappingsInternal(IMapperConfigurationExpression mapper)
        {
            //Permission
            mapper.CreateMap<Permission, FlatPermissionDto>();
            mapper.CreateMap<Permission, FlatPermissionWithLevelDto>();

            //UserBill
            mapper.CreateMap<CreateOrUpdateBillConfigInputDto, BillConfig>();
            mapper.CreateMap<CreateOrUpdateUserBillInputDto, UserBill>();

            //Asset

            // Apartment
            mapper.CreateMap<CreateApartmentInput, Apartment>();

            // Apartment status
            mapper.CreateMap<CreateApartmentStatusInput, ApartmentStatus>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateApartmentStatusInput, ApartmentStatus>()
                .ForMember(dest => dest.Name, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Code, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ColorCode, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));

            // Floor 
            mapper.CreateMap<UpdateFloorInput, Floor>()
                .ForMember(dest => dest.DisplayName, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Description, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.BuildingId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.UrbanId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            // Apartment 
            mapper.CreateMap<UpdateApartmentInput, Apartment>()
                .ForMember(dest => dest.Name, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ImageUrl, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ApartmentCode, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Properties, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Area, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.BlockId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.FloorId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.TypeId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.StatusId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ProvinceCode, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.DistrictCode, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.WardCode, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Address, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Description, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.BuildingId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.UrbanId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            // ApartmentHistory 
            mapper.CreateMap<UpdateApartmentHistoryDto, ApartmentHistory>()
                .ForMember(dest => dest.TenantId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ApartmentId, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Title, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Description, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Type, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ImageUrls, opt => opt.Ignore())
                .ForMember(dest => dest.FileUrls, opt => opt.Ignore())
                .ForMember(dest => dest.ExecutorName, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ExecutorPhone, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ExecutorEmail, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.SupervisorName, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.SupervisorPhone, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.SupervisorEmail, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ReceiverName, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest=> dest.ReceiverPhone, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest=> dest.ReceiverEmail, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Cost, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.DateStart, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.DateEnd, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            // ApartmentType
            mapper.CreateMap<UpdateApartmentTypeInput, ApartmentType>()
                .ForMember(dest => dest.Name, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Code, opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            // BlockTower 
            mapper.CreateMap<UpdateBlockTowerInput, BlockTower>()
                .ForMember(dest => dest.DisplayName,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Code,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.UrbanId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.BuildingId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Description,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            // Building 
            mapper.CreateMap<UpdateBuildingDto, AppOrganizationUnit>()
                .ForMember(dest => dest.Id,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.DisplayName,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ParentId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ProjectCode,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Description,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ImageUrl,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Email,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Area,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Address,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ProvinceCode,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.DistrictCode,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.WardCode,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.NumberFloor,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            // Citizen Reflect
            mapper.CreateMap<CreateCitizenReflectInput, CitizenReflect>();
           
            // MeterType
            mapper.CreateMap<CreateMeterTypeInput, MeterType>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<CreateMeterTypeByUserInput, MeterType>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<UpdateMeterTypeInput, MeterType>()
                .ForMember(dest => dest.Name,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Description,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<UpdateMeterTypeByUserInput, MeterType>()
                .ForMember(dest => dest.Name,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Description,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));

            // Meter
            mapper.CreateMap<CreateMeterInput, Meter>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<CreateMeterByUserInput, Meter>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<UpdateMeterInput, Meter>()
                .ForMember(dest => dest.Name,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.QrCode,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Code,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.MeterTypeId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.UrbanId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.BuildingId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ApartmentCode,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<UpdateMeterByUserInput, Meter>()
                .ForMember(dest => dest.Name,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.QrCode,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Code,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.MeterTypeId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.UrbanId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.BuildingId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.ApartmentCode,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));

             // MeterMonthly
            mapper.CreateMap<CreateMeterMonthlyInput, MeterMonthly>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<CreateMeterMonthlyByUserInput, MeterMonthly>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<UpdateMeterMonthlyInput, MeterMonthly>()
                .ForMember(dest => dest.MeterId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Period,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Value,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<UpdateMeterMonthlyByUserInput, MeterMonthly>()
                .ForMember(dest => dest.MeterId,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Period,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Value,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));


        }

        #region method helpers 
        private static bool IsNotNullOrDefault<T>(T srcMember)
        {
            if (srcMember is IEnumerable list)
            {
                return list.GetEnumerator().MoveNext();
            }
            return srcMember != null && !EqualityComparer<T>.Default.Equals(srcMember, default);
        }
        #endregion
    }
}