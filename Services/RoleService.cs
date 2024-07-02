using Microsoft.Graph;

namespace EventManagementApi.Services
{
    public class RoleService
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IConfiguration _configuration;

        public RoleService(GraphServiceClient graphServiceClient, IConfiguration configuration)
        {
            _graphServiceClient = graphServiceClient;
            _configuration = configuration;
        }

        public async Task<Guid> GetRoleIdByNameAsync(string roleName)
        {
            var servicePrincipal = await _graphServiceClient.ServicePrincipals[_configuration["EntraId:ClientId"]]
                .GetAsync();

            if (servicePrincipal == null || servicePrincipal.AppRoles == null)
            {
                throw new ArgumentException("Service principal not found.");
            }

            var appRole = servicePrincipal.AppRoles.FirstOrDefault(r => r.DisplayName!.Equals(roleName, StringComparison.OrdinalIgnoreCase));

            if (appRole == null || !appRole.Id.HasValue)
            {
                throw new ArgumentException($"Role {roleName} not found.");
            }

            return appRole.Id.Value;
        }
    }
}
