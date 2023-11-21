#nullable enable
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.Application;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using IMAX.Organizations;
using IMAX.QueriesExtension;
using IMAX.Services.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public interface IUserApartmentAppService : IApplicationService
    {
        Task<DataResult> GetAllApartmentAsync(GetAllApartmentByUserInput input);
        Task<DataResult> GetApartmentDetailAsync(long id);
    }
    public class UserApartmentAppService : IMAXAppServiceBase, IUserApartmentAppService
    {
        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnit;
        private readonly IRepository<ApartmentStatus, long> _statusRepository;
        private readonly IRepository<BlockTower, long> _blockRepository;
        private readonly IRepository<ApartmentType, long> _typeRepository;
        private readonly IRepository<Floor, long> _floorRepository;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepository;

        public UserApartmentAppService(
            IRepository<Apartment, long> apartmentRepository,
            IRepository<ApartmentStatus, long> statusRepository,
            IRepository<Floor, long> floorRepository,
            IRepository<ApartmentType, long> typeRepository,
            IRepository<BlockTower, long> blockRepository,
            IRepository<AppOrganizationUnit, long> organizationUnit,
            IRepository<CitizenTemp, long> citizenTempRepository
            )
        {
            _apartmentRepository = apartmentRepository;
            _floorRepository = floorRepository;
            _organizationUnit = organizationUnit;
            _statusRepository = statusRepository;
            _blockRepository = blockRepository;
            _typeRepository = typeRepository;
            _citizenTempRepository = citizenTempRepository;
        }


        public async Task<DataResult> GetAllApartmentAsync(GetAllApartmentByUserInput input)
        {
            try
            {
                IQueryable<GetAllApartmentDto> query = (from apartment in _apartmentRepository.GetAll()
                                                        select new GetAllApartmentDto
                                                        {
                                                            Id = apartment.Id,
                                                            ApartmentCode = apartment.ApartmentCode,
                                                            Name = apartment.Name,
                                                            Area = apartment.Area,
                                                            ImageUrl = apartment.ImageUrl,
                                                            BuildingId = apartment.BuildingId,
                                                            UrbanId = apartment.UrbanId,
                                                            BuildingName = _organizationUnit.GetAll().Where(x => x.Id == apartment.BuildingId).Select(x => x.DisplayName).FirstOrDefault(),
                                                            UrbanName = _organizationUnit.GetAll().Where(x => x.Id == apartment.UrbanId).Select(x => x.DisplayName).FirstOrDefault(),
                                                            StatusId = apartment.StatusId,
                                                            TypeId = apartment.TypeId,
                                                            CreationTime = apartment.CreationTime,
                                                            OwnerName = _citizenTempRepository.GetAll()
                                                                    .Where(x => x.ApartmentCode == apartment.ApartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                                                                    .Select(x => x.FullName).FirstOrDefault(),
                                                            OwnerPhoneNumber = _citizenTempRepository.GetAll()
                                                                    .Where(x => x.ApartmentCode == apartment.ApartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                                                                    .Select(x => x.PhoneNumber).FirstOrDefault(),
                                                            TypeName = _typeRepository.GetAll().Where(x => x.Id == apartment.TypeId).Select(x => x.Name).FirstOrDefault(),
                                                            StatusName = _statusRepository.GetAll().Where(x => x.Id == apartment.StatusId).Select(x => x.Name).FirstOrDefault(),
                                                            NumberOfMember = _citizenTempRepository.GetAll().Where(x => x.IsStayed == true && x.ApartmentCode == apartment.ApartmentCode).Count(),
                                                            FloorId = apartment.FloorId,
                                                            FloorName = _floorRepository.GetAll().Where(x => x.Id == apartment.FloorId).Select(x => x.DisplayName).FirstOrDefault(),
                                                        })
                         .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                         .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                         .WhereIf(input.StatusId.HasValue, x => x.StatusId == input.StatusId)
                         .WhereIf(input.TypeId.HasValue, x => x.TypeId == input.TypeId)
                         .ApplySearchFilter(input.Keyword, x => x.Name, x => x.ApartmentCode);

                List<GetAllApartmentDto> result = await query
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByApartment.APARTMENT_CODE) // sort default
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> GetApartmentDetailAsync(long id)
        {
            try
            {
                Apartment? apartment = await _apartmentRepository.FirstOrDefaultAsync(id);
                if (apartment == null)
                {
                    return DataResult.ResultSuccess(null, "Get apartment detail success!");
                }
                GetApartmentDetailDto apartmentDto = apartment.MapTo<GetApartmentDetailDto>();

                // Get FloorName, StatusName, and TypeName
                apartmentDto.BuildingName = GetOrganizationName(apartmentDto.BuildingId);
                apartmentDto.UrbanName = GetOrganizationName(apartmentDto.UrbanId);
                apartmentDto.FloorName = GetFloorName(apartmentDto.FloorId);
                apartmentDto.StatusName = GetStatusName(apartmentDto.StatusId);
                apartmentDto.TypeName = GetTypeName(apartmentDto.TypeId);
                apartmentDto.BlockName = GetBlockName(apartmentDto.BlockId);

                // Get list of members
                apartmentDto.Members = GetMembers(apartmentDto.ApartmentCode);

                // Get owner's name and phone number
                CitizenTemp owner = GetOwner(apartmentDto.ApartmentCode).FirstOrDefault();
                apartmentDto.OwnerName = owner?.FullName;
                apartmentDto.OwnerPhoneNumber = owner?.PhoneNumber;

                return DataResult.ResultSuccess(apartmentDto, "Get apartment detail success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        #region method helper
        private string? GetFloorName(long? floorId)
        {
            return _floorRepository.FirstOrDefault(floorId ?? 0)?.DisplayName;
        }
        private string? GetStatusName(long? statusId)
        {
            return _statusRepository.FirstOrDefault(statusId ?? 0)?.Name;
        }
        private string? GetBlockName(long? blockId)
        {
            return _blockRepository.FirstOrDefault(blockId ?? 0)?.DisplayName;
        }
        private string? GetTypeName(long typeId)
        {
            return _typeRepository.FirstOrDefault(typeId)?.Name;
        }

        private List<MemberOfApartmentDto> GetMembers(string apartmentCode)
        {
            return _citizenTempRepository
                .GetAll()
                .Where(x => x.ApartmentCode == apartmentCode)
                .Select(x => new MemberOfApartmentDto
                {
                    Id = x.Id,
                    Generation = x.OwnerGeneration,
                    Name = x.FullName,
                    PhoneNumber = x.PhoneNumber,
                    Relationship = x.RelationShip,
                    IsStayed = x.IsStayed,
                })
                .ToList();
        }

        private IQueryable<CitizenTemp> GetOwner(string apartmentCode)
        {
            return _citizenTempRepository
                .GetAll()
                .Where(x => x.ApartmentCode == apartmentCode && x.IsStayed == true && x.RelationShip == RELATIONSHIP.Contractor);
        }
        private string? GetOrganizationName(long? organizationId)
        {
            return _organizationUnit.GetAll().Where(x => x.Id == (organizationId ?? 0)).Select(x => x.DisplayName).FirstOrDefault();
        }
        #endregion
    }
}
