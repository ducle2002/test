namespace Yootek.Authorization.Permissions.Dto
{
    public class GetAllPermissionsDto
    {
        public int? TenantId { get; set; }

        public GetAllPermissionsDto()
        {
        }

        public GetAllPermissionsDto(int? tenantId)
        {
            TenantId = tenantId;
        }
    }

    public class AddPermissionForTenantDto
    {
        public int TenantId { get; set; }
        public string[] Permissions { get; set; }
    }

    public class UpdatePermissionsForTenantDto
    {
        public int TenantId { get; set; }
        public string[] Permissions { get; set; }
    }

    public class DeletePermissionFromTenantDto
    {
        public int TenantId { get; set; }
        public string Permission { get; set; }
    }
}