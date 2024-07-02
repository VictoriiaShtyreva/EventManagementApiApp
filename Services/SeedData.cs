// using EventManagementApi.DTO;
// using EventManagementApi.Entities;
// using EventManagementApi.Entity;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Graph;
// using Microsoft.Graph.Models;

// namespace EventManagementApi.Services
// {
//     public class SeedData
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly GraphServiceClient _graphServiceClient;
//         private readonly RoleService _roleService;
//         private readonly IConfiguration _configuration;

//         public SeedData(ApplicationDbContext context, GraphServiceClient graphServiceClient, RoleService roleService, IConfiguration configuration)
//         {
//             _context = context;
//             _graphServiceClient = graphServiceClient;
//             _roleService = roleService;
//             _configuration = configuration;
//         }

//         public async Task SeedDataAsync()
//         {
//             await _context.Database.MigrateAsync();

//             if (!await _context.Events.AnyAsync())
//             {
//                 await SeedUsersAsync();
//                 await SeedEventsAsync();
//             }
//         }

//         private async Task SeedUsersAsync()
//         {
//             var users = new[]
//             {
//                 new UserRegistrationDto { DisplayName = "Admin User", UserPrincipalName = "adminuser", Password = "Password123!", Role = "Admin" },
//                 new UserRegistrationDto { DisplayName = "Event Provider", UserPrincipalName = "eventprovider", Password = "Password123!", Role = "EventProvider" },
//                 new UserRegistrationDto { DisplayName = "Regular User", UserPrincipalName = "regularuser", Password = "Password123!", Role = "User" }
//             };

//             foreach (var userDto in users)
//             {
//                 var user = new User
//                 {
//                     AccountEnabled = true,
//                     DisplayName = userDto.DisplayName,
//                     MailNickname = userDto.UserPrincipalName,
//                     UserPrincipalName = $"{userDto.UserPrincipalName}@{_configuration["EntraId:Domain"]}",
//                     PasswordProfile = new PasswordProfile
//                     {
//                         ForceChangePasswordNextSignIn = false,
//                         Password = userDto.Password
//                     }
//                 };

//                 var createdUser = await _graphServiceClient.Users.PostAsync(user);

//                 var appRoleAssignment = new AppRoleAssignment
//                 {
//                     PrincipalId = Guid.Parse(createdUser!.Id!),
//                     ResourceId = Guid.Parse(_configuration["EntraId:ClientId"]!),
//                     AppRoleId = await _roleService.GetRoleIdByNameAsync(userDto.Role!)
//                 };

//                 await _graphServiceClient.Users[createdUser.Id].AppRoleAssignments.PostAsync(appRoleAssignment);
//             }
//         }

//         private async Task SeedEventsAsync()
//         {
//             var events = new[]
//             {
//                 new Events { Id = Guid.NewGuid(), Name = "Tech Conference 2024", Description = "Annual tech conference", Location = "New York", Date = DateTime.UtcNow.AddMonths(1), OrganizerId = "1" },
//                 new Events { Id = Guid.NewGuid(), Name = "Music Festival", Description = "Outdoor music festival", Location = "Los Angeles", Date = DateTime.UtcNow.AddMonths(2), OrganizerId = "2" },
//                 new Events { Id = Guid.NewGuid(), Name = "Art Exhibition", Description = "Modern art exhibition", Location = "Chicago", Date = DateTime.UtcNow.AddMonths(3), OrganizerId = "3" }
//             };

//             _context.Events.AddRange(events);
//             await _context.SaveChangesAsync();
//         }
//     }
// }
