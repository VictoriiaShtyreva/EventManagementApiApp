using System.Security.Claims;
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

        public AccountsController(GraphServiceClient graphServiceClient, IConfiguration configuration)
        {
            _graphServiceClient = graphServiceClient;
            _configuration = configuration;
        }

        // Register a new user
        [HttpPost("register")]
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

            try
            {
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
                    ResourceId = Guid.Parse(_configuration["EntraId:ClientId"] ?? throw new InvalidOperationException("ClientId configuration is missing")),
                    AppRoleId = Guid.Parse(_configuration["EntraId:AppRoles:User"]!) // Assign "User" role by default
                };

                await _graphServiceClient.Users[createdUser.Id].AppRoleAssignments.PostAsync(appRoleAssignment);

                return Ok(new { Message = "User registered successfully with role assigned" });
            }
            catch (ServiceException ex)
            {
                return BadRequest($"Error creating user: {ex.Message}");
            }
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

        // Get user profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User ID not found in token." });
            }

            var user = await _graphServiceClient.Users[userId].GetAsync();

            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var userProfile = new
            {
                user.DisplayName,
                user.UserPrincipalName,
                user.Mail,
                user.JobTitle,
                user.MobilePhone,
                user.OfficeLocation
            };

            return Ok(userProfile);
        }
    }

}
