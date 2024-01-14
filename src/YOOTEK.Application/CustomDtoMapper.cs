using AutoMapper;
using Yootek.Authorization.Permissions.Dto;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.SmartCommunity.Apartment;
using Yootek.Yootek.EntityDb.SmartCommunity.Phidichvu;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Building.Dto;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Organizations;
using Yootek.Services;
using Yootek.Services.Dto;
using System.Collections;
using System.Collections.Generic;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.Authorization.Users;
using Yootek.Common.Enum;
using Yootek.Yootek.EntityDb.Clb.Enterprise;
using Yootek.Yootek.EntityDb.Clb.Event;
using Yootek.Yootek.EntityDb.Clb.Jobs;
using Yootek.Yootek.EntityDb.Clb.Projects;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Yootek.Service;
using Permission = Abp.Authorization.Permission;
using System.Reflection;
using System;
using AutoMapper.Internal;
using AutoMapper.Configuration;
using System.Linq;

namespace Yootek
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
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.BillType,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
                
            mapper.CreateMap<UpdateMeterTypeByUserInput, MeterType>()
                .ForMember(dest => dest.Name,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.Description,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest => dest.BillType,
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

            mapper.CreateMap<CreateForumPostDto, ForumPost>()
                .ForMember(dest => dest.State,
                        opt => opt.MapFrom( src => CommonENumForum.FORUM_STATE.NEW));

            //=====change========
            mapper.CreateMap<UpdateForumPostDto, ForumPost>()
                .ForMember(dest=> dest.State,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForMember(dest=>dest.Type ,
                    opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));

            mapper.CreateMap<CreateForumCommentDto, ForumComment>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateForumCommentDto, ForumComment>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            //=====change========

            mapper.CreateMap<CreateMemberByUserDto, Member>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateMemberByUserDto, Member>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<CreateMemberByAdminDto, Member>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateMemberByAdminDto, Member>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<CreateJobDto, Jobs>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateJobDto, Jobs>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));

            mapper.CreateMap<CreateProjectDto, Projects>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateProjectDto, Projects>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<Projects, ProjectDto>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<User, ShortenedUserDto>();
            
            mapper.CreateMap<CreateReactionDto, ForumPostReaction>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateReactionDto, ForumPostReaction>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<CreateEnterpriseDto, Enterprises>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateEnterpriseDto, Enterprises>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<CreateClbEvent, ClbEvent>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateClbEvent, ClbEvent>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<ClbEvent, ClbEventDto>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));

            //=====change========
            mapper.CreateMap<CreateClbEventCommentDto, ClbEventComment>()
                .ForMember(x=>x.IsLike, opt => opt.MapFrom(src => false))
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateClbEventCommentDto, ClbEventComment>()
                .ForMember(x=>x.IsLike, opt =>opt.Ignore())
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<CreateClbEventFollowDto, ClbEventComment>()
                .ForMember(x => x.Comment, opt => opt.Ignore())
                .ForMember(x=>x.IsLike, opt => opt.MapFrom(src => true))
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateClbEventFollowDto, ClbEventComment>()
                .ForMember(x=>x.IsLike, opt =>opt.Ignore())
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            //=====change========

            mapper.CreateMap<CreateEnterpriseDto, Enterprises>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateEnterpriseDto, Enterprises>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<CreateBusinessFieldDto, BusinessField>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateBusinessFieldDto, BusinessField>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateEmployeeDto, EmployeeDto>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            mapper.CreateMap<CreateForumTopicDto, ForumTopic>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            mapper.CreateMap<UpdateForumTopicDto, ForumTopic>()
                .ForAllMembers(opt=> opt.Condition((src, dest, srcMember) => IsNotNullOrDefault(srcMember)));
            
            // Ecofarm
            mapper.CreateMap<User, UserInfoDto>();
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

    public static class AutoMapperExtensions
    {
        private static readonly PropertyInfo TypeMapActionsProperty = typeof(TypeMapConfiguration).GetProperty("TypeMapActions", BindingFlags.NonPublic | BindingFlags.Instance);

        // not needed in AutoMapper 12.0.1
        private static readonly PropertyInfo DestinationTypeDetailsProperty = typeof(TypeMap).GetProperty("DestinationTypeDetails", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void ForAllOtherMembers<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression, Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
        {
            var typeMapConfiguration = (TypeMapConfiguration)expression;

            var typeMapActions = (List<Action<TypeMap>>)TypeMapActionsProperty.GetValue(typeMapConfiguration);

            typeMapActions.Add(typeMap =>
            {
                var destinationTypeDetails = (TypeDetails)DestinationTypeDetailsProperty.GetValue(typeMap);

                foreach (var accessor in destinationTypeDetails.WriteAccessors.Where(m => typeMapConfiguration.GetDestinationMemberConfiguration(m) == null))
                {
                    expression.ForMember(accessor.Name, memberOptions);
                }
            });
        }
    }
}