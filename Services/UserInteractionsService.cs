using EventManagementApi.Entity;
using Microsoft.Azure.Cosmos;

namespace EventManagementApi.Services
{
    public class UserInteractionsService
    {
        private readonly Container _container;
        private readonly Container _eventsContainer;

        public UserInteractionsService(CosmosClient cosmosClient, IConfiguration configuration)
        {
            var databaseName = configuration["CosmosDb:DatabaseName"];
            var userInteractionsContainerName = configuration["CosmosDb:UserInteractionsContainer"];
            var eventsContainerName = configuration["CosmosDb:EventsContainer"];
            _container = cosmosClient.GetContainer(databaseName, userInteractionsContainerName);
            _eventsContainer = cosmosClient.GetContainer(databaseName, eventsContainerName);
        }

        public async Task<IEnumerable<Event>> GetMostViewedEventsAsync()
        {
            var query = new QueryDefinition("SELECT c.eventId, COUNT(c.id) as views FROM c WHERE c.interactionType = 'view' GROUP BY c.eventId ORDER BY views DESC");

            var iterator = _container.GetItemQueryIterator<UserInteraction>(query);
            var results = new List<UserInteraction>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            var eventIds = results.Select(r => r.EventId).Distinct();
            var events = new List<Event>();

            foreach (var eventId in eventIds)
            {
                var eventResponse = await _eventsContainer.ReadItemAsync<Event>(eventId, new PartitionKey(eventId));
                events.Add(eventResponse.Resource);
            }

            return events;
        }

        public async Task AddUserInteractionAsync(UserInteraction userInteraction)
        {
            await _container.CreateItemAsync(userInteraction, new PartitionKey(userInteraction.EventId));
        }
    }
}