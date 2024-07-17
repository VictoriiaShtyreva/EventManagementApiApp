using System.Text;
using Azure.Messaging.ServiceBus;
using EventManagementApi.DTO;
using EventManagementApi.Entities;
using EventManagementApi.Entity;
using EventManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EventManagementApi.Controllers
{
    [Route("api/v1/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EventMetadataService _eventMetadataService;
        private readonly UserInteractionsService _userInteractionsService;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string? _queueName;

        public EventsController(ApplicationDbContext context, IConfiguration configuration, EventMetadataService eventMetadataService, UserInteractionsService userInteractionsService, ServiceBusClient serviceBusClient)
        {
            _context = context;
            _eventMetadataService = eventMetadataService;
            _userInteractionsService = userInteractionsService;
            _serviceBusClient = serviceBusClient;
            _queueName = configuration["ServiceBus:QueueName"];
        }

        // Accessible by all authenticated users
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                var events = await _context.Events.ToListAsync();
                if (events.Count == 0)
                {
                    return Ok(new { Message = "No events registered yet." });
                }
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching events.", Details = ex.Message });
            }
        }

        // Accessible by Event Providers
        [HttpPost]
        [Authorize(Policy = "EventProviderOnly")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto newEventDto)
        {
            try
            {
                var newEvent = new Event
                {
                    Id = Guid.NewGuid(),
                    Name = newEventDto.Name,
                    Description = newEventDto.Description,
                    Location = newEventDto.Location,
                    Date = newEventDto.Date,
                    OrganizerId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                // Add event metadata
                var eventMetadata = new EventMetadata
                {
                    Id = Guid.NewGuid().ToString(),
                    EventId = newEvent.Id.ToString(),
                    Type = newEventDto.Type,
                    Category = newEventDto.Category
                };
                await _eventMetadataService.AddEventMetadataAsync(eventMetadata);

                return Ok(new { Message = "Event created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the event.", Details = ex.Message });
            }
        }

        // Accessible by all authenticated users
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    return NotFound(new { Message = "Event not found." });
                }
                return Ok(eventItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the event.", Details = ex.Message });
            }
        }

        // Accessible by Event Providers
        [HttpPatch("{id}")]
        [Authorize(Policy = "EventProviderOnly")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventUpdateDto updatedEventDto)
        {
            try
            {
                var eventToUpdate = await _context.Events.FindAsync(id);
                if (eventToUpdate == null)
                {
                    return NotFound(new { Message = "Event not found." });
                }

                eventToUpdate.Name = updatedEventDto.Name;
                eventToUpdate.Description = updatedEventDto.Description;
                eventToUpdate.Location = updatedEventDto.Location;
                eventToUpdate.Date = updatedEventDto.Date;

                _context.Events.Update(eventToUpdate);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Event updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the event.", Details = ex.Message });
            }
        }

        // Accessible by Admins
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                var eventToDelete = await _context.Events.FindAsync(id);
                if (eventToDelete == null)
                {
                    return NotFound(new { Message = "Event not found." });
                }

                _context.Events.Remove(eventToDelete);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Event deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the event.", Details = ex.Message });
            }
        }

        // User can register for an event
        [HttpPost("{id}/register")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> RegisterForEvent(Guid id)
        {
            try
            {
                var userId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
                var registration = new EventRegistration
                {
                    EventId = id.ToString(),
                    UserId = userId,
                    RegistrationDate = DateTime.UtcNow
                };

                _context.EventRegistrations.Add(registration);
                await _context.SaveChangesAsync();

                // Add user interaction
                var userInteraction = new UserInteraction
                {
                    Id = Guid.NewGuid().ToString(),
                    EventId = id.ToString(),
                    InteractionType = "register",
                    UserId = userId
                };
                await _userInteractionsService.AddUserInteractionAsync(userInteraction);

                // Send event registration message to queue
                var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userInteraction));
                var message = new ServiceBusMessage(messageBody) { SessionId = userId };

                ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);
                await sender.SendMessageAsync(message);

                return Ok(new { Message = "Registered for event successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while registering for the event.", Details = ex.Message });
            }
        }

        // User can unregister from an event
        [HttpDelete("{id}/unregister")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> UnregisterFromEvent(Guid id)
        {
            try
            {
                var userId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

                var registration = await _context.EventRegistrations
                    .FirstOrDefaultAsync(r => r.EventId == id.ToString() && r.UserId == userId);

                if (registration == null)
                {
                    return NotFound(new { Message = "Registration not found." });
                }

                _context.EventRegistrations.Remove(registration);
                await _context.SaveChangesAsync();

                // Add user interaction
                var userInteraction = new UserInteraction
                {
                    Id = Guid.NewGuid().ToString(),
                    EventId = id.ToString(),
                    InteractionType = "unregister",
                    UserId = userId
                };
                await _userInteractionsService.AddUserInteractionAsync(userInteraction);

                // Send event unregistration message to queue
                var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userInteraction));
                var message = new ServiceBusMessage(messageBody) { SessionId = userId };

                ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);
                await sender.SendMessageAsync(message);

                return Ok(new { Message = "Unregistered from event successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while unregistering from the event.", Details = ex.Message });
            }
        }

        // Search events by metadata (Cosmos DB NoSQL)
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchEventsByMetadata([FromQuery] string type, [FromQuery] string category)
        {
            try
            {
                var results = await _eventMetadataService.SearchEventsByMetadataAsync(type, category);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while searching for events.", Details = ex.Message });
            }
        }

        // Get most viewed events (Cosmos DB NoSQL)
        [HttpGet("most-viewed")]
        [Authorize]
        public async Task<IActionResult> GetMostViewedEvents()
        {
            try
            {
                var results = await _userInteractionsService.GetMostViewedEventsAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the most viewed events.", Details = ex.Message });
            }
        }
    }
}
