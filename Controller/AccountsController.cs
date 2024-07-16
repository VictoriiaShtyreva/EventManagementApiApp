using EventManagementApi.DTO;
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
        public AccountsController(GraphServiceClient graphServiceClient, IConfiguration configuration)
        {
            _graphServiceClient = graphServiceClient;
            _configuration = configuration;
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

                return Ok(new { Message = "User registered successfully with user-role assigned" });
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

    }
}
