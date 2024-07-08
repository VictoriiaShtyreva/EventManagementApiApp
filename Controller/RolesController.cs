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
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleDto model)
        {
            var user = await _graphServiceClient.Users[model.UserId].GetAsync();
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var appRoleAssignment = new AppRoleAssignment
            {
                PrincipalId = Guid.Parse(model.UserId!),
                ResourceId = Guid.Parse(_configuration["EntraId:ServicePrincipalId"] ?? throw new InvalidOperationException("ClientId configuration is missing")),
                AppRoleId = await _roleService.GetRoleIdByNameAsync(model.Role ?? throw new ArgumentNullException(nameof(model.Role)))
            };

            await _graphServiceClient.Users[model.UserId].AppRoleAssignments.PostAsync(appRoleAssignment);
            return Ok(new { Message = $"Role {model.Role} for {model.UserId} assigned successfully" });
        }

        // Remove a role from a user
        [HttpDelete("remove")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> RemoveRole([FromBody] UserRoleDto model)
        {
            var user = await _graphServiceClient.Users[model.UserId].GetAsync();
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var appRoleAssignmentsPage = await _graphServiceClient.Users[model.UserId].AppRoleAssignments.GetAsync();
            if (appRoleAssignmentsPage == null || appRoleAssignmentsPage.Value == null)
            {
                return NotFound(new { Message = "No role assignments found for this user" });
            }

            var appRoleAssignments = appRoleAssignmentsPage.Value;
            var appRoleId = await _roleService.GetRoleIdByNameAsync(model.Role ?? throw new ArgumentNullException(nameof(model.Role)));

            var assignmentToRemove = appRoleAssignments.FirstOrDefault(a => a.AppRoleId == appRoleId);

            if (assignmentToRemove == null)
            {
                return NotFound(new { Message = "Role assignment not found" });
            }

            await _graphServiceClient.Users[model.UserId].AppRoleAssignments[assignmentToRemove.Id].DeleteAsync();
            return Ok(new { Message = "Role removed successfully" });
        }
    }
}
