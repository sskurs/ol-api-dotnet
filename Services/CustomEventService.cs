using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using System.Text.Json;

namespace ol_api_dotnet.Services
{
    public class CustomEventService
    {
        private readonly AppDbContext _context;

        public CustomEventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomEvent> CreateEventAsync(CustomEvent customEvent)
        {
            _context.CustomEvents.Add(customEvent);
            await _context.SaveChangesAsync();
            return customEvent;
        }

        public async Task<List<CustomEvent>> GetUserEventsAsync(int userId, int page = 1, int limit = 10)
        {
            return await _context.CustomEvents
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<CustomEvent> CreateEventAsync(int userId, string type, Dictionary<string, object> data)
        {
            var customEvent = new CustomEvent
            {
                UserId = userId,
                Type = type,
                Data = JsonDocument.Parse(JsonSerializer.Serialize(data)),
                CreatedAt = DateTime.UtcNow
            };

            _context.CustomEvents.Add(customEvent);
            await _context.SaveChangesAsync();
            return customEvent;
        }
    }

    public class CustomEventResult
    {
        public int EventId { get; set; }
        public int PointsAwarded { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
} 