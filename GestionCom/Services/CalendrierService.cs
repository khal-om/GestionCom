using GestionCom.Data;
using GestionCom.DTOs.Calendrier;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class CalendrierService
    {
        private readonly AppDbContext _context;

        public CalendrierService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateEventAsync(CreateCalendarEventDto dto, int userId)
        {
            if (dto == null)
                throw new Exception("Données invalides.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new Exception("La description est requise.");

            var eventDate = NormalizeToUtc(dto.EventDate);

            var calendarEvent = new CalendarEvent
            {
                EventDate = eventDate,
                Description = dto.Description.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            return calendarEvent.Id;
        }

        public async Task<CalendarOverviewDto> GetOverviewAsync()
        {
            var today = DateTime.Today;

            var eventEntities = await _context.CalendarEvents
                .AsNoTracking()
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            var creatorIds = eventEntities
                .Where(e => e.CreatedBy.HasValue)
                .Select(e => e.CreatedBy!.Value)
                .Distinct()
                .ToList();

            var userNames = await _context.Users
                .AsNoTracking()
                .Where(u => creatorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            var events = eventEntities
                .Select(e => new CalendarEventItemDto
                {
                    Id = e.Id,
                    EventDate = e.EventDate,
                    Description = e.Description,
                    CreatedByName = e.CreatedBy.HasValue && userNames.TryGetValue(e.CreatedBy.Value, out var username)
                        ? username
                        : "Système",
                    IsToday = e.EventDate.Date == today,
                    IsPaymentReminder = e.Description.StartsWith("Échéance ", StringComparison.OrdinalIgnoreCase),
                    ReminderBadge = e.Description.StartsWith("Échéance ", StringComparison.OrdinalIgnoreCase)
                        ? "Paiement en attente"
                        : null
                })
                .ToList();

            return new CalendarOverviewDto
            {
                UpcomingPaymentReminders = events
                    .Where(e => e.EventDate.Date >= today && e.IsPaymentReminder)
                    .OrderBy(e => e.EventDate)
                    .ToList(),
                UpcomingManualEvents = events
                    .Where(e => e.EventDate.Date >= today && !e.IsPaymentReminder)
                    .OrderBy(e => e.EventDate)
                    .ToList(),
                UpcomingEvents = events
                    .Where(e => e.EventDate.Date >= today)
                    .OrderBy(e => e.EventDate)
                    .ToList(),
                PastEvents = events
                    .Where(e => e.EventDate.Date < today)
                    .OrderByDescending(e => e.EventDate)
                    .ToList()
            };
        }

        public async Task UpdateEventAsync(int eventId, CreateCalendarEventDto dto, int userId)
        {
            if (dto == null)
                throw new InvalidOperationException("Données invalides.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new InvalidOperationException("La description est requise.");

            var calendarEvent = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id == eventId);

            if (calendarEvent == null)
                throw new InvalidOperationException("Événement introuvable.");

            calendarEvent.EventDate = NormalizeToUtc(dto.EventDate);
            calendarEvent.Description = dto.Description.Trim();
            calendarEvent.UpdatedAt = DateTime.UtcNow;
            calendarEvent.UpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}
