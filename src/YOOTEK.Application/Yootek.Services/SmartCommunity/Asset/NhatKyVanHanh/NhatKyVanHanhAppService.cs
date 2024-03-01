using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.CommonENum;
namespace Yootek.Services
{
    public interface INhatKyVanHanhAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllNhatKyVanHanhInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(NhatKyVanHanhCreateDto input);
        Task<DataResult> Update(NhatKyVanHanhDto input);
        Task<DataResult> UpdateStatus(long id, int status);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class NhatKyVanHanhAppService : YootekAppServiceBase, INhatKyVanHanhAppService
    {
        private readonly IRepository<NhatKyVanHanh, long> _repository;
        private readonly IRepository<TaiSanChiTiet, long> _taiSanChiTietRepository;
        private readonly IRepository<BlockTower, long> _blockTowersRepository;
        private readonly IRepository<Apartment, long> _apartmentsRepository;
        private readonly IRepository<AppOrganizationUnit, long> _abpOrganizationUnitsRepository;
        private readonly IRepository<Floor, long> _floorsRepository;
        private readonly IRepository<User, long> _userRepository;


        public NhatKyVanHanhAppService(IRepository<NhatKyVanHanh, long> repository,
IRepository<TaiSanChiTiet, long> taiSanChiTietRepository, IRepository<BlockTower, long> blockTowersRepository,
            IRepository<Apartment, long> apartmentsRepository,
            IRepository<AppOrganizationUnit, long> abpOrganizationUnitsRepository,
            IRepository<User, long> userRepository,
            IRepository<Floor, long> floorsRepository)
        {
            _repository = repository;
            _taiSanChiTietRepository = taiSanChiTietRepository;
            _blockTowersRepository = blockTowersRepository;
            _apartmentsRepository = apartmentsRepository;
            _abpOrganizationUnitsRepository = abpOrganizationUnitsRepository;
            _floorsRepository = floorsRepository;
            _userRepository = userRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllNhatKyVanHanhInputDto input)
        {
            try
            {
                IQueryable<NhatKyVanHanhDto> query = (from o in _repository.GetAll()
                                                      select new NhatKyVanHanhDto
                                                      {
                                                          Id = o.Id,
                                                          TaiSanId = o.TaiSanId,
                                                          BoPhanTheoDoi = o.BoPhanTheoDoi,
                                                          NgaySuaChua = o.NgaySuaChua,
                                                          NgayCheckList = o.NgayCheckList,
                                                          TenantId = o.TenantId,
                                                          TrangThai = o.TrangThai,
                                                          NguoiKiemTraId = o.NguoiKiemTraId,
                                                          CreationTime = o.CreationTime,
                                                          NguoiKiemTraText = _userRepository.GetAll().Where(x => x.Id == o.NguoiKiemTraId).Select(x => x.FullName).FirstOrDefault(),
                                                          TaiSanText = _taiSanChiTietRepository.GetAll().Where(x => x.Id == o.TaiSanId).Select(x => x.Title).FirstOrDefault(),
                                                      })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.BoPhanTheoDoi.ToLower().Contains(input.Keyword.ToLower()))
                .WhereIf(input.TaiSanId > 0, x => x.TaiSanId == input.TaiSanId)
                .WhereIf(input.NguoiKiemTraId > 0, x => x.NguoiKiemTraId == input.NguoiKiemTraId)
                .WhereIf(input.TrangThai > 0, x => x.TrangThai == input.TrangThai)
                ;
                List<NhatKyVanHanhDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess, query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetById(long id)
        {
            try
            {
                var item = await _repository.GetAsync(id);
                var data = item.MapTo<NhatKyVanHanhDto>();
                var taisan = _taiSanChiTietRepository.GetAll().Where(x => x.Id == data.TaiSanId).FirstOrDefault();
                if (taisan != null)
                {
                    data.BlockText = !taisan.BlockId.HasValue ? "" : _blockTowersRepository.GetAll().Where(x => x.Id == taisan.BlockId.Value).Select(x => x.DisplayName).FirstOrDefault();
                    data.ApartmentText = !taisan.ApartmentId.HasValue ? "" : _apartmentsRepository.GetAll().Where(x => x.Id == taisan.ApartmentId.Value).Select(x => x.ApartmentCode).FirstOrDefault();
                    data.BuildingText = !taisan.BuildingId.HasValue ? "" : _abpOrganizationUnitsRepository.GetAll().Where(x => x.Id == taisan.BuildingId.Value).Select(x => x.DisplayName).FirstOrDefault();
                    data.FloorText = !taisan.FloorId.HasValue ? "" : _floorsRepository.GetAll().Where(x => x.Id == taisan.FloorId.Value).Select(x => x.DisplayName).FirstOrDefault();
                    data.TaiSanText = taisan.Code + "-" + taisan.Title;
                }
                data.NguoiKiemTraText = _userRepository.GetAll().Where(x => x.Id == data.NguoiKiemTraId).Select(x => x.FullName).FirstOrDefault();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(NhatKyVanHanhCreateDto dto)
        {
            try
            {
                if(dto.TaiSanId>0)
                {
                    NhatKyVanHanh item = new NhatKyVanHanh()
                    {
                        TaiSanId = dto.TaiSanId,
                        NguoiKiemTraId = dto.NguoiKiemTraId,
                        NgayCheckList = dto.NgayCheckList,
                        NgaySuaChua = dto.NgaySuaChua,
                        TenantId = AbpSession.TenantId,
                        TrangThai = (int)TrangThaiNhatKyVanHanh.TaoMoi,
                        NoiDung = dto.NoiDung,
                        BoPhanTheoDoi = dto.BoPhanTheoDoi
                    };
                    await _repository.InsertAsync(item);
                    return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
                }
                else if(dto.LstIdTaiSan.Count>0)
                {
                    foreach(var taiSanId in dto.LstIdTaiSan)
                    {
                        NhatKyVanHanh item = new NhatKyVanHanh()
                        {
                            TaiSanId = taiSanId,
                            NguoiKiemTraId = dto.NguoiKiemTraId,
                            NgayCheckList = dto.NgayCheckList,
                            NgaySuaChua = dto.NgaySuaChua,
                            TenantId = AbpSession.TenantId,
                            TrangThai = (int)TrangThaiNhatKyVanHanh.TaoMoi,
                            NoiDung = dto.NoiDung,
                            BoPhanTheoDoi = dto.BoPhanTheoDoi
                        };                        
                        await _repository.InsertAsync(item);
                    }
                    return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
                }    
                else
                {
                    return DataResult.ResultFail("Không có tài sản");
                }    
                //NhatKyVanHanh item = dto.MapTo<NhatKyVanHanh>();
                //item.TenantId = AbpSession.TenantId;
                //item.TrangThai = (int)TrangThaiNhatKyVanHanh.TaoMoi;
                //await _repository.InsertAsync(item);
                
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Update(NhatKyVanHanhDto dto)
        {
            try
            {
                NhatKyVanHanh item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    dto.MapTo(item);
                    item.TenantId = AbpSession.TenantId;
                    await _repository.UpdateAsync(item);
                    return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.UpdateSuccess);
                }
                else
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> UpdateStatus(long id, int status)
        {
            try
            {
                NhatKyVanHanh item = await _repository.GetAsync(id);
                if (item != null)
                {
                    item.TrangThai = status;
                    await _repository.UpdateAsync(item);
                    return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.UpdateSuccess);
                }
                else
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Delete(long id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                var data = DataResult.ResultSuccess(Common.Resource.QuanLyChung.DeleteSuccess);
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
