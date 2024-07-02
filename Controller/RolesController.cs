using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using EventManagementApi.DTO;

namespace EventManagementApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IConfiguration _configuration;

        public RolesController(GraphServiceClient graphServiceClient, IConfiguration configuration)
        {
            _graphServiceClient = graphServiceClient;
            _configuration = configuration;
        }

        // Assign a role to a user
        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleUpdateDto model)
        {
            var user = await _graphServiceClient.Users[model.UserId].GetAsync();
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var appRoleAssignment = new AppRoleAssignment
            {
                PrincipalId = Guid.Parse(model.UserId!),
                ResourceId = Guid.Parse(_configuration["EntraId:ClientId"] ?? throw new InvalidOperationException("ClientId configuration is missing")),
                AppRoleId = GetRoleIdByName(model.Role ?? throw new ArgumentNullException(nameof(model.Role)))
            };

            await _graphServiceClient.Users[model.UserId].AppRoleAssignments.PostAsync(appRoleAssignment);
            return Ok(new { Message = "Role assigned successfully" });
        }

        // Remove a role from a user
        [HttpPost("remove")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole([FromBody] UserRoleUpdateDto model)
        {
            var user = await _graphServiceClient.Users[model.UserId].GetAsync();
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var appRoleAssignments = await _graphServiceClient.Users[model.UserId].AppRoleAssignments.GetAsync();
            var assignmentToRemove = appRoleAssignments.CurrentPage.Find(a => a.AppRoleId == GetRoleIdByName(model.Role ?? throw new ArgumentNullException(nameof(model.Role))));

            if (assignmentToRemove == null)
            {
                return NotFound(new { Message = "Role assignment not found" });
            }

            await _graphServiceClient.Users[model.UserId].AppRoleAssignments[assignmentToRemove.Id].DeleteAsync();
            return Ok(new { Message = "Role removed successfully" });
        }

        private Guid GetRoleIdByName(string roleName)
        {
            return roleName switch
            {
                "Admin" => Guid.Parse("0102246e-4126-4df6-8908-5e85879af2df"),
                "EventProvider" => Guid.Parse("1df7e13c-c41f-4a47-a097-ae78ffed3062"),
                "User" => Guid.Parse("2f883966-3c87-4363-b367-7e86a7438018"),
                _ => throw new ArgumentException("Invalid role name")
            };
        }
    }
}
