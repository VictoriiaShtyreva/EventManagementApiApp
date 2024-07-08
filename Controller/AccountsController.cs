using EventManagementApi.DTO;
using EventManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web.Resource;

namespace EventManagementApi.Controllers
{
    [Route("api/v1/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IConfiguration _configuration;
        private readonly RoleService _roleService;

        public AccountsController(GraphServiceClient graphServiceClient, IConfiguration configuration, RoleService roleService)
        {
            _graphServiceClient = graphServiceClient;
            _configuration = configuration;
            _roleService = roleService;
        }

        // Register a new user
        [HttpPost("register")]
        [RequiredScopeOrAppPermission(
           RequiredScopesConfigurationKey = "EntraId:Scopes:Write",
           RequiredAppPermissionsConfigurationKey = "EntraId:AppPermissions:Write"
       )]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
        {
            var tenantId = _configuration["EntraId:TenantId"];
            var user = new User
            {
                AccountEnabled = true,
                DisplayName = registrationDto.DisplayName,
                MailNickname = registrationDto.UserPrincipalName,
                UserPrincipalName = $"{registrationDto.UserPrincipalName}@{_configuration["EntraId:Domain"]}",
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = registrationDto.Password
                }
            };

            // Create the user
            var createdUser = await _graphServiceClient.Users.PostAsync(user);

            if (createdUser == null || createdUser.Id == null)
            {
                return BadRequest("Failed to create user.");
            }

            // Assign role to the user
            var appRoleAssignment = new AppRoleAssignment
            {
                PrincipalId = Guid.Parse(createdUser.Id),
                ResourceId = Guid.Parse(_configuration["EntraId:ServicePrincipalId"] ?? throw new InvalidOperationException("ClientId configuration is missing")),
                AppRoleId = await _roleService.GetRoleIdByNameAsync("User") // Assign "User" role by default
            };

            await _graphServiceClient.Users[createdUser.Id].AppRoleAssignments.PostAsync(appRoleAssignment);

            return Ok(new { Message = "User registered successfully with role assigned" });
        }

        // Get user profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            var user = await _graphServiceClient.Users[userId].GetAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new { user.DisplayName, user.UserPrincipalName, user.Mail });
        }

        // Update user profile
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto model)
        {
            var userId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            var user = new User
            {
                DisplayName = model.DisplayName,
                MailNickname = model.UserPrincipalName
            };

            await _graphServiceClient.Users[userId].PatchAsync(user);

            return Ok(new { Message = "User profile updated successfully" });
        }


        // Delete a user
        [HttpDelete("delete/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            await _graphServiceClient.Users[userId].DeleteAsync();
            return Ok(new { Message = "User deleted successfully" });
        }
    }

}
