namespace EventManagementApi.DTO
{
    public class UserRegistrationDto
    {
        public string? DisplayName { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }  // Role can be Admin, EventProvider, or User
    }

    public class UserProfileUpdateDto
    {
        public string? DisplayName { get; set; }
        public string? UserPrincipalName { get; set; }
    }
}