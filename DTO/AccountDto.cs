namespace EventManagementApi.DTO
{
    public class AccountCreateDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; } // Role can be User, Admin, or EventProvider
    }

    public class AccountUpdateDto
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}