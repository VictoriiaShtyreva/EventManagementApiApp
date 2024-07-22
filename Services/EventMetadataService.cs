using EventManagementApi.Entity;
using Microsoft.Azure.Cosmos;

namespace EventManagementApi.Services
{
    public class EventMetadataService
    {
        private readonly Container _container;

        public EventMetadataService(CosmosClient cosmosClient, IConfiguration configuration)
        {
            var databaseName = configuration["CosmosDb:DatabaseName"];
            var containerName = configuration["CosmosDb:EventMetadataContainer"];
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<EventMetadata>> SearchEventsByCriteriaAsync(string queryText, params (string Name, object Value)[] parameters)
        {
            var query = new QueryDefinition(queryText);
            foreach (var parameter in parameters)
            {
                query = query.WithParameter(parameter.Name, parameter.Value);
            }

            var iterator = _container.GetItemQueryIterator<EventMetadata>(query);
            var results = new List<EventMetadata>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<IEnumerable<EventMetadata>> SearchEventsByTypeAndCategoryAsync(string type, string category)
        {
            var queryText = "SELECT * FROM c WHERE c.type = @type AND c.category = @category";
            return await SearchEventsByCriteriaAsync(queryText, ("@type", type), ("@category", category));
        }

        public async Task<IEnumerable<EventMetadata>> SearchEventsByEventIdAsync(string eventId)
        {
            var queryText = "SELECT * FROM c WHERE c.eventId = @eventId";
            return await SearchEventsByCriteriaAsync(queryText, ("@eventId", eventId));
        }

        public async Task AddEventMetadataAsync(EventMetadata eventMetadata)
        {
            await _container.CreateItemAsync(eventMetadata, new PartitionKey(eventMetadata.EventId));
        }

        public async Task DeleteEventMetadataAsync(string eventMetadataId)
        {
            await _container.DeleteItemAsync<EventMetadata>(eventMetadataId, new PartitionKey(eventMetadataId));
        }

    }
}