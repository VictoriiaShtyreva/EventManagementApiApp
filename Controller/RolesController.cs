using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using EventManagementApi.DTO;
using EventManagementApi.Services;

namespace EventManagementApi.Controllers
{
    [Route("api/v1/roles")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly RoleService _roleService;
        private readonly IConfiguration _configuration;

        public RolesController(GraphServiceClient graphServiceClient, RoleService roleService, IConfiguration configuration)
        {
            _graphServiceClient = graphServiceClient;
            _roleService = roleService;
            _configuration = configuration;
        }

        // Assign a role to a user
        [HttpPost("assign")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleDto model)
        {
            var user = await _graphServiceClient.Users[model.UserId].GetAsync();
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Get the Service Principal ID and New Role ID
            var servicePrincipalId = _configuration["EntraId:ServicePrincipalId"];
            var newRoleId = await _roleService.GetRoleIdByNameAsync(model.Role ?? throw new ArgumentNullException(nameof(model.Role)));

            // Remove existing role assignments
            var appRoleAssignmentsResponse = await _graphServiceClient.Users[model.UserId].AppRoleAssignments.GetAsync();

            if (appRoleAssignmentsResponse?.Value != null)
            {
                // Remove existing role assignments
                foreach (var assignment in appRoleAssignmentsResponse.Value)
                {
                    if (assignment.ResourceId.ToString() == servicePrincipalId)
                    {
                        await _graphServiceClient.Users[model.UserId].AppRoleAssignments[assignment.Id].DeleteAsync();
                    }
                }
            }

            // Assign the new role to the user
            var appRoleAssignment = new AppRoleAssignment
            {
                PrincipalId = Guid.Parse(model.UserId!),
                ResourceId = Guid.Parse(_configuration["EntraId:ServicePrincipalId"] ?? throw new InvalidOperationException("ClientId configuration is missing")),
                AppRoleId = newRoleId,
            };

            await _graphServiceClient.Users[model.UserId].AppRoleAssignments.PostAsync(appRoleAssignment);
            return Ok(new { Message = $"Role {model.Role} for assigned successfully" });
        }

    }
}
