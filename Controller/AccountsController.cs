using EventManagementApi.DTO;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace EventManagementApi.Controllers
{
    [Route("api/v1/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IConfiguration _configuration;
        private readonly TelemetryClient _telemetryClient;
        public AccountsController(GraphServiceClient graphServiceClient, IConfiguration configuration, TelemetryClient telemetryClient)
        {
            _graphServiceClient = graphServiceClient;
            _configuration = configuration;
            _telemetryClient = telemetryClient;
        }

        // Register a new user
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
        {
            var domain = _configuration["EntraId:Domain"];
            var ServicePrincipalId = _configuration["EntraId:ServicePrincipalId"];
            var userRoleId = _configuration["EntraId:AppRoles:User"];

            var user = new User
            {
                AccountEnabled = true,
                DisplayName = registrationDto.DisplayName,
                MailNickname = registrationDto.UserPrincipalName,
                UserPrincipalName = $"{registrationDto.UserPrincipalName}@{domain}",
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = registrationDto.Password
                }
            };

            try
            {
                // Create the user in Azure AD
                var createdUser = await _graphServiceClient.Users.PostAsync(user);

                if (createdUser == null || createdUser.Id == null)
                {
                    return BadRequest("Failed to create user.");
                }

                // Assign role to the user within your application
                var appRoleAssignment = new AppRoleAssignment
                {
                    PrincipalId = Guid.Parse(createdUser.Id),
                    ResourceId = Guid.Parse(ServicePrincipalId ?? throw new InvalidOperationException("ClientId configuration is missing")),
                    AppRoleId = Guid.Parse(userRoleId ?? throw new InvalidOperationException("UserRoleId configuration is missing"))
                };

                await _graphServiceClient.Users[createdUser.Id].AppRoleAssignments.PostAsync(appRoleAssignment);

                _telemetryClient.TrackEvent("UserRegistered", new Dictionary<string, string>
                {
                    { "DisplayName", createdUser.DisplayName! },
                    { "UserPrincipalName", createdUser.UserPrincipalName! }
                });

                return Ok(new { Message = $"User {createdUser.DisplayName} registered successfully with user-role assigned. For login use your user principal name as {createdUser.UserPrincipalName} and your password." });
            }
            catch (ServiceException ex)
            {
                return BadRequest($"Error creating user: {ex.Message}");
            }
        }

        // Get a list of users
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetUsers()
        {
            var users = new List<UserDto>();

            var result = await _graphServiceClient.Users.GetAsync();
            if (result != null && result.Value != null)
            {
                users.AddRange(result.Value.Select(user => new UserDto
                {
                    Id = user.Id,
                    UserPrincipalName = user.UserPrincipalName
                }));
                return Ok(users);
            }
            return NotFound("Users not found");

        }

        // Delete a user
        [HttpDelete("delete/{userId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            await _graphServiceClient.Users[userId].DeleteAsync();
            return Ok(new { Message = "User deleted successfully" });
        }

        // Add email address for receiving confirmation emails about registration/unregistration to/from event
        [HttpPost("update-email")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> UpdateEmail([FromBody] UserEmailDto userEmailDto)
        {
            try
            {
                var user = new User
                {
                    Mail = userEmailDto.Email
                };

                await _graphServiceClient.Users[userEmailDto.UserId].PatchAsync(user);

                _telemetryClient.TrackEvent("UserEmailUpdated", new Dictionary<string, string>
                {
                    { "UserId", userEmailDto.UserId!},
                    { "Email", userEmailDto.Email! }
                });

                return Ok(new { Message = "Email address updated successfully" });
            }
            catch (ServiceException ex)
            {
                return BadRequest($"Error updating email address: {ex.Message}");
            }
        }

    }
}
