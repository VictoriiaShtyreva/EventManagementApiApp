using Dapper;
using EventManagementApi.DTO;
using EventManagementApi.Entity;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Npgsql;

namespace EventManagementApi.Services
{
    public class UserInteractionsService
    {
        private readonly Container _container;
        private readonly Container _eventsContainer;
        private readonly string? _postgresConnectionString;

        public UserInteractionsService(CosmosClient cosmosClient, IConfiguration configuration)
        {
            var databaseName = configuration["CosmosDb:DatabaseName"];
            var userInteractionsContainerName = configuration["CosmosDb:UserInteractionsContainer"];
            var eventsContainerName = configuration["CosmosDb:EventMetadataContainer"];
            _container = cosmosClient.GetContainer(databaseName, userInteractionsContainerName);
            _eventsContainer = cosmosClient.GetContainer(databaseName, eventsContainerName);
            _postgresConnectionString = configuration["ConnectionStrings:DefaultConnection"];
        }

        public async Task<IEnumerable<EventWithRegistrationCountDto>> GetMostRegisteredEventsAsync()
        {
            var query = new QueryDefinition("SELECT c.eventId, COUNT(1) as userCount FROM c WHERE c.InteractionType = 'Register' GROUP BY c.eventId");

            var iterator = _container.GetItemQueryIterator<dynamic>(query);
            var eventRegistrations = new List<(string EventId, int UserCount)>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                eventRegistrations.AddRange(response.Select(r => (EventId: (string)r.eventId, UserCount: (int)r.userCount)));
            }

            var distinctEventIds = eventRegistrations.Select(er => er.EventId).Distinct();
            var events = new List<EventWithRegistrationCountDto>();

            using (var connection = new NpgsqlConnection(_postgresConnectionString))
            {
                await connection.OpenAsync();

                foreach (var eventId in distinctEventIds)
                {
                    var eventQuery = "SELECT \"Id\", \"Name\", \"Description\", \"Location\", \"Date\" FROM \"Events\" WHERE \"Id\" = @EventId";
                    var eventDetails = await connection.QuerySingleOrDefaultAsync<EventWithRegistrationCountDto>(eventQuery, new { EventId = Guid.Parse(eventId) });

                    if (eventDetails != null)
                    {
                        eventDetails.RegisteredUserCount = eventRegistrations.First(er => er.EventId == eventId).UserCount;
                        events.Add(eventDetails);
                    }
                }
            }

            return events;
        }


        public async Task AddUserInteractionAsync(UserInteraction userInteraction)
        {
            await _container.CreateItemAsync(userInteraction, new PartitionKey(userInteraction.EventId));
        }

        public async Task<UserInteraction> GetUserInteractionAsync(string eventId, string userId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.eventId = @eventId AND c.UserId = @userId")
                .WithParameter("@eventId", eventId)
                .WithParameter("@userId", userId);

            var iterator = _container.GetItemQueryIterator<UserInteraction>(query);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var userInteraction = response.FirstOrDefault();
                if (userInteraction != null)
                {
                    return userInteraction;
                }
            }
            return null!;
        }

        public async Task DeleteUserInteractionAsync(string eventId, string userId)
        {
            var userInteraction = await GetUserInteractionAsync(eventId, userId);
            await _container.DeleteItemAsync<UserInteraction>(userInteraction.Id, new PartitionKey(userInteraction.EventId));
        }
    }
}