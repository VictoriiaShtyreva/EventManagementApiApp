using EventManagementApi.DTO;
using EventManagementApi.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace EventManagementApi.Controllers
{
    [Route("api/v1/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly Container _registrationContainer;
        private readonly IConfiguration _configuration;

        public EventsController(CosmosClient cosmosClient, IConfiguration configuration)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;
            _container = _cosmosClient.GetContainer(_configuration["CosmosDb:DatabaseName"], "Events");
            _registrationContainer = _cosmosClient.GetContainer(_configuration["CosmosDb:DatabaseName"], "EventRegistrations");
        }

        // Accessible by all authenticated users
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var resultSet = _container.GetItemQueryIterator<Events>(query);

            var events = new List<Events>();
            while (resultSet.HasMoreResults)
            {
                var response = await resultSet.ReadNextAsync();
                events.AddRange(response);
            }

            return Ok(events);
        }

        // Accessible by Event Providers
        [HttpPost]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto newEventDto)
        {
            var newEvent = new Events
            {
                Id = Guid.NewGuid(),
                Name = newEventDto.Name,
                Description = newEventDto.Description,
                Location = newEventDto.Location,
                Date = newEventDto.Date,
                OrganizerId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            };

            await _container.CreateItemAsync(newEvent);
            return Ok(new { Message = "Event created successfully" });
        }

        // Accessible by all authenticated users
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            try
            {
                var eventResponse = await _container.ReadItemAsync<Events>(id.ToString(), new PartitionKey(id.ToString()));
                return Ok(eventResponse.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }

        // Accessible by Event Providers
        [HttpPut("{id}")]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventUpdateDto updatedEventDto)
        {
            try
            {
                var eventResponse = await _container.ReadItemAsync<Events>(id.ToString(), new PartitionKey(id.ToString()));
                var eventToUpdate = eventResponse.Resource;

                eventToUpdate.Name = updatedEventDto.Name;
                eventToUpdate.Description = updatedEventDto.Description;
                eventToUpdate.Location = updatedEventDto.Location;
                eventToUpdate.Date = updatedEventDto.Date;

                await _container.ReplaceItemAsync(eventToUpdate, eventToUpdate.Id.ToString());
                return Ok(new { Message = "Event updated successfully" });
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }

        // Accessible by Admins
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                await _container.DeleteItemAsync<Events>(id.ToString(), new PartitionKey(id.ToString()));
                return Ok(new { Message = "Event deleted successfully" });
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }

        // User can register for an event
        [HttpPost("{id}/register")]
        [Authorize(Policy = "User")]
        public async Task<IActionResult> RegisterForEvent(Guid id)
        {
            var userId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            var registration = new EventRegistrations
            {
                EventId = id,
                UserId = userId
            };

            await _registrationContainer.CreateItemAsync(registration);
            return Ok(new { Message = "Registered for event successfully" });
        }

        // User can unregister from an event
        [HttpDelete("{id}/unregister")]
        [Authorize(Policy = "User")]
        public async Task<IActionResult> UnregisterFromEvent(Guid id)
        {
            var userId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            var partitionKey = new PartitionKey(id.ToString());
            var registration = _registrationContainer.GetItemLinqQueryable<EventRegistrations>()
                .Where(r => r.EventId == id && r.UserId == userId)
                .AsEnumerable()
                .FirstOrDefault();

            if (registration == null)
            {
                return NotFound();
            }

            await _registrationContainer.DeleteItemAsync<EventRegistrations>(registration.EventId.ToString(), partitionKey);
            return Ok(new { Message = "Unregistered from event successfully" });
        }
    }
}
