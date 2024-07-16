namespace EventManagementApi.DTO
{
    public class UserRegistrationDto
    {
        public string? DisplayName { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? Password { get; set; }
    }

    public class UserProfileUpdateDto
    {
        public string? DisplayName { get; set; }
        public string? UserPrincipalName { get; set; }
    }

    public class UserDto
    {
        public string? Id { get; set; }
        public string? UserPrincipalName { get; set; }
    }
}