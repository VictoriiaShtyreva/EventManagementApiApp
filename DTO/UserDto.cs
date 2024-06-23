using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
}