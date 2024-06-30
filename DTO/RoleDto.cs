namespace EventManagementApi.DTO
{
    public class RoleCreateDto
    {
        public string? RoleName { get; set; }
    }

    public class UserRoleUpdateDto
    {
        public string? UserId { get; set; }
        public string? Role { get; set; }
    }
}