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

        public async Task<IEnumerable<EventMetadata>> SearchEventsByMetadataAsync(string type, string category)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.type = @type AND c.category = @category")
            .WithParameter("@type", type)
            .WithParameter("@category", category);

            var iterator = _container.GetItemQueryIterator<EventMetadata>(query);
            var results = new List<EventMetadata>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task AddEventMetadataAsync(EventMetadata eventMetadata)
        {
            await _container.CreateItemAsync(eventMetadata, new PartitionKey(eventMetadata.EventId));
        }
    }
}