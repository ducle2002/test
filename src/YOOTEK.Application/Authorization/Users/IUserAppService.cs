using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using IMAX.Authorization.Users;
using IMAX.Roles.Dto;
using IMAX.Users.Dto;

namespace IMAX.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task DeActivate(EntityDto<long> user);
        Task Activate(EntityDto<long> user);
        Task<ListResultDto<RoleDto>> GetRoles();
        Task ChangeLanguage(ChangeUserLanguageDto input);
        Task UpdateProfilePicture(UpdateProfilePictureInput input);
        Task<bool> ChangePassword(ChangePasswordDto input);
        Task MapToEntityDto(User user);
        Task CreateAsync(User user);
    }
}
