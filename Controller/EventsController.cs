using System.Text;
using Azure.Messaging.ServiceBus;
using EventManagementApi.DTO;
using EventManagementApi.Entities;
using EventManagementApi.Entity;
using EventManagementApi.Services;
using Microsoft.ApplicationInsights;
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
        private readonly ServiceBusQueueService _serviceBusQueueService;
        private readonly EventBlobService _eventBlobService;
        private readonly TelemetryClient _telemetryClient;

        public EventsController(ApplicationDbContext context, IConfiguration configuration, EventMetadataService eventMetadataService, UserInteractionsService userInteractionsService, ServiceBusQueueService serviceBusQueueService, EventBlobService eventBlobService, TelemetryClient telemetryClient)
        {
            _context = context;
            _eventMetadataService = eventMetadataService;
            _userInteractionsService = userInteractionsService;
            _serviceBusQueueService = serviceBusQueueService;
            _eventBlobService = eventBlobService;
            _telemetryClient = telemetryClient;
        }

        // Accessible by all authenticated users - GET ALL EVENTS
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                var events = await _context.Events
                    .Include(e => e.EventImages)
                    .Include(e => e.EventDocuments)
                    .ToListAsync();

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

        // Accessible by Event Providers - CREATE EVENT
        [HttpPost]
        [Authorize(Policy = "EventProviderOnly")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto newEventDto)
        {
            try
            {
                var organizerId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

                var newEvent = new Event
                {
                    Id = Guid.NewGuid(),
                    Name = newEventDto.Name,
                    Description = newEventDto.Description,
                    Location = newEventDto.Location,
                    Date = newEventDto.Date,
                    OrganizerId = organizerId
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

                return Ok(new { Message = $"Event: {newEvent.Name}, Type: {eventMetadata.Type}, Category: {eventMetadata.Category} created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the event.", Details = ex.Message });
            }
        }

        // Accessible by all authenticated users - GET EVENT BY ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    return NotFound(new { Message = $"Event: {eventItem?.Name} not found." });
                }
                return Ok(eventItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the event.", Details = ex.Message });
            }
        }

        // Accessible by Event Providers - UPDATE EVENT
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

                eventToUpdate.Name = updatedEventDto.Name ?? eventToUpdate.Name;
                eventToUpdate.Description = updatedEventDto.Description ?? eventToUpdate.Description;
                eventToUpdate.Location = updatedEventDto.Location ?? eventToUpdate.Location;
                eventToUpdate.Date = updatedEventDto.Date ?? eventToUpdate.Date;

                _context.Events.Update(eventToUpdate);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Event: {eventToUpdate.Name} updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the event.", Details = ex.Message });
            }
        }

        // Accessible by Admins - DELETE EVENT
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

                //Delete event metadata
                var eventMetadata = await _eventMetadataService.SearchEventsByEventIdAsync(id.ToString());
                if (eventMetadata != null)
                {
                    await _eventMetadataService.DeleteEventMetadataAsync(id.ToString());
                }
                else
                {
                    return NotFound(new { Message = "Event metadata not found for this event." });
                }

                _context.Events.Remove(eventToDelete);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Event: {eventToDelete.Name} deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the event.", Details = ex.Message });
            }
        }

        // User can register for an event - USER REGISTER 
        [HttpPost("{id}/register")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> RegisterForEvent(Guid id)
        {
            try
            {
                var userId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

                var registrationDto = new EventRegistrationDto
                {
                    EventId = id,
                    UserId = userId,
                    Action = "Register"
                };

                var registration = new EventRegistration
                {
                    EventId = registrationDto.EventId,
                    UserId = registrationDto.UserId,
                    Action = registrationDto.Action
                };

                // Check user interaction
                var existingInteraction = await _userInteractionsService.GetUserInteractionAsync(id.ToString(), userId!);
                if (existingInteraction != null)
                {
                    return BadRequest(new { Message = "User is already registered for this event." });
                }
                else
                {
                    var userInteraction = new UserInteraction
                    {
                        Id = Guid.NewGuid().ToString(),
                        EventId = registrationDto.EventId.ToString(),
                        InteractionType = "Register",
                        UserId = registrationDto.UserId
                    };
                    await _userInteractionsService.AddUserInteractionAsync(userInteraction);
                }

                // Send event registration message to BusQueue
                var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(registration));
                var message = new ServiceBusMessage(messageBody) { SessionId = userId };
                await _serviceBusQueueService.SendMessageAsync(message);

                return Ok(new { Message = $" User {registration.UserId} successfully sent request to register for event {registration.EventId}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while registering for the event.", Details = ex.Message });
            }
        }

        // User can unregister from an event - USER UNREGISTER
        [HttpDelete("{id}/unregister")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> UnregisterFromEvent(Guid id)
        {
            try
            {
                var userId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
                if (userId == null)
                {
                    return BadRequest(new { Message = "User ID is missing." });
                }

                // Check if user interaction exists and delete it
                var existingInteraction = await _userInteractionsService.GetUserInteractionAsync(id.ToString(), userId!);
                if (existingInteraction != null)
                {
                    await _userInteractionsService.DeleteUserInteractionAsync(id.ToString(), userId!);
                }
                else
                {
                    return NotFound(new { Message = "User interaction not found for this event." });
                }

                var unRegistrationDto = new EventRegistrationDto
                {
                    EventId = id,
                    UserId = userId,
                    Action = "Unregister"
                };

                // Send event registration message to BusQueue
                var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(unRegistrationDto));
                var message = new ServiceBusMessage(messageBody) { SessionId = userId };
                await _serviceBusQueueService.SendMessageAsync(message);

                return Ok(new { Message = $"User {unRegistrationDto.UserId} successfully sent request to unregister from event {unRegistrationDto.EventId}" });
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
                var results = await _eventMetadataService.SearchEventsByTypeAndCategoryAsync(type, category);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while searching for events.", Details = ex.Message });
            }
        }

        // Get most register events (Cosmos DB NoSQL)
        [HttpGet("most-register")]
        [Authorize]
        public async Task<IActionResult> GetMostRegisteredEvents()
        {
            try
            {
                var results = await _userInteractionsService.GetMostRegisteredEventsAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the most viewed events.", Details = ex.Message });
            }
        }

        // Upload Event Images
        [HttpPost("{id}/upload-images")]
        [Authorize(Policy = "EventProviderOnly")]
        public async Task<IActionResult> UploadEventImages(Guid id, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files selected");
            }

            var uploadedUrls = new List<string>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                var fileName = $"{id}/{file.FileName}";
                using (var stream = file.OpenReadStream())
                {
                    var result = await _eventBlobService.UploadEventImageAsync(stream, fileName);
                    if (result.Success)
                    {
                        var eventImage = new EventImage
                        {
                            Id = Guid.NewGuid(),
                            EventId = id,
                            Url = result.Url
                        };

                        _context.EventImages.Add(eventImage);
                        uploadedUrls.Add(result.Url!);

                        // Log custom event to Application Insights
                        _telemetryClient.TrackEvent("EventImageUploaded", new Dictionary<string, string>
                        {
                            { "EventId", id.ToString() },
                            { "ImageUrl", result.Url! }
                        });
                    }
                    else
                    {
                        errors.Add(result.ErrorMessage!);
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (errors.Any())
            {
                return StatusCode(500, new { UploadedUrls = uploadedUrls, Errors = errors });
            }

            return Ok(new { ImageUrls = uploadedUrls });
        }

        // Upload Event Documents
        [HttpPost("{id}/upload-documents")]
        [Authorize(Policy = "EventProviderOnly")]
        public async Task<IActionResult> UploadEventDocuments(Guid id, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files selected");
            }

            var uploadedUrls = new List<string>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                var fileName = $"{id}/{file.FileName}";
                using (var stream = file.OpenReadStream())
                {
                    var result = await _eventBlobService.UploadEventDocumentAsync(stream, fileName);
                    if (result.Success)
                    {
                        var eventDocument = new EventDocument
                        {
                            Id = Guid.NewGuid(),
                            EventId = id,
                            Url = result.Url
                        };

                        _context.EventDocuments.Add(eventDocument);
                        uploadedUrls.Add(result.Url!);

                        // Log custom event to Application Insights
                        _telemetryClient.TrackEvent("EventDocumentUploaded", new Dictionary<string, string>
                        {
                            { "EventId", id.ToString() },
                            { "DocumentUrl", result.Url! }
                        });
                    }
                    else
                    {
                        errors.Add(result.ErrorMessage!);
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (errors.Any())
            {
                return StatusCode(500, new { UploadedUrls = uploadedUrls, Errors = errors });
            }

            return Ok(new { DocumentUrls = uploadedUrls });
        }

        // Get Event Images
        [HttpGet("{id}/images")]
        [Authorize]
        public async Task<IActionResult> GetEventImages(Guid id)
        {
            var eventImages = await _context.EventImages.Where(ei => ei.EventId == id).ToListAsync();
            if (eventImages.Count == 0)
            {
                return NotFound("No images found for this event.");
            }

            var imageUrls = eventImages.Select(ei => ei.Url).ToList();
            return Ok(imageUrls);
        }

        // Get Event Documents
        [HttpGet("{id}/documents")]
        [Authorize]
        public async Task<IActionResult> GetEventDocuments(Guid id)
        {
            var eventDocuments = await _context.EventDocuments.Where(ed => ed.EventId == id).ToListAsync();
            if (eventDocuments.Count == 0)
            {
                return NotFound("No documents found for this event.");
            }
            var documentUrls = eventDocuments.Select(ed => ed.Url).ToList();
            return Ok(documentUrls);
        }

        // Download Event Image
        [HttpGet("{id}/image/{fileName}")]
        [Authorize]
        public async Task<IActionResult> DownloadEventImage(Guid id, string fileName)
        {
            var result = await _eventBlobService.DownloadEventImageAsync($"{id}/{fileName}");
            if (result.Success)
            {
                return File(result.FileStream!, "application/octet-stream", fileName);
            }
            return StatusCode(500, result.ErrorMessage);
        }

        // Download Event Document
        [HttpGet("{id}/document/{fileName}")]
        [Authorize]
        public async Task<IActionResult> DownloadEventDocument(Guid id, string fileName)
        {
            var result = await _eventBlobService.DownloadEventDocumentAsync($"{id}/{fileName}");
            if (result.Success)
            {
                return File(result.FileStream!, "application/octet-stream", fileName);
            }
            return StatusCode(500, result.ErrorMessage);
        }
    }
}
