using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using System.Linq;
using Abp.Linq.Extensions;
using IMAX.Application;
using Microsoft.EntityFrameworkCore;

namespace IMAX.Services
{
    public interface IUserHotlineService : IApplicationService
    {
        Task<object> GetAllHotlineAsync(ListHotlineInputForUserDto input);
    }
    public class UserHotlineService: IMAXAppServiceBase, IUserHotlineService
    {
        private readonly IRepository<Hotlines, long> _hotlineRepos;

        public UserHotlineService(IRepository<Hotlines, long> hotlineRepos)
        {
            _hotlineRepos = hotlineRepos;
        }

        public async Task<object> GetAllHotlineAsync(ListHotlineInputForUserDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from dt in _hotlineRepos.GetAll()
                            select new GetInputHotline()
                            {
                                Id = dt.Id,
                                TenantId = dt.TenantId,
                                Name = dt.Name,
                                OrganizationUnitId = dt.OrganizationUnitId,
                                Properties = dt.Properties,
                                UrbanId = dt.UrbanId,
                                BuildingId = dt.BuildingId
                            })
                        .Where(u => u.UrbanId == input.UrbanId)
                        .WhereIf(input.BuildingId != null, u => u.BuildingId == input.BuildingId || u.BuildingId == null)
                        .ApplySearchFilter(input.Keyword, x => x.Name, x => x.Properties);

                    var result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByHotline.NAME)
                        .PageBy(input).ToListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
                }
            }
            catch(Exception e)
            { 
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
    }
}