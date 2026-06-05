using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly MongoDbContext _context;

        public EventsController(MongoDbContext context)
        {
            _context = context;
        }

        // SEARCH
        public async Task<IActionResult> Search(string venue, int? eventTypeId, DateTime? startDate, DateTime? endDate)
        {
            var filter = Builders<Event>.Filter.Empty;

            // Venue filter
            if (!string.IsNullOrEmpty(venue))
                filter &= Builders<Event>.Filter.Regex(e => e.Venue, new MongoDB.Bson.BsonRegularExpression(venue, "i"));

            // EventType filter
            if (eventTypeId.HasValue)
                filter &= Builders<Event>.Filter.Eq(e => e.EventTypeId, eventTypeId);

            // Date range filter
            if (startDate.HasValue && endDate.HasValue)
                filter &= Builders<Event>.Filter.And(
                    Builders<Event>.Filter.Gte(e => e.StartDate, startDate),
                    Builders<Event>.Filter.Lte(e => e.EndDate, endDate)
                );

            var events = await _context.Events.Find(filter).ToListAsync();

            // Venue availability filter (ensures no overlapping events at same venue)
            events = events.Where(e => !events.Any(ev =>
                ev.Venue == e.Venue &&
                ev.EventID != e.EventID &&
                ev.StartDate < e.EndDate &&
                ev.EndDate > e.StartDate)).ToList();

            // Pass EventTypes to view for dropdown
            ViewBag.EventTypes = await _context.EventTypes.Find(Builders<EventType>.Filter.Empty).ToListAsync();

            return View(events);
        }

        // INDEX
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.Find(Builders<Event>.Filter.Empty).ToListAsync();
            return View(events);
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.Find(e => e.EventID == id).FirstOrDefaultAsync();
            if (ev == null) return NotFound();

            return View(ev);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,EventName,Description,Venue,StartDate,EndDate,EventTypeId")] Event ev)
        {
            if (ModelState.IsValid)
            {
                await _context.Events.InsertOneAsync(ev);
                return RedirectToAction(nameof(Index));
            }
            return View(ev);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.Find(e => e.EventID == id).FirstOrDefaultAsync();
            if (ev == null) return NotFound();

            return View(ev);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventID,EventName,Description,Venue,StartDate,EndDate,EventTypeId")] Event ev)
        {
            if (id != ev.EventID) return NotFound();

            if (ModelState.IsValid)
            {
                var filter = Builders<Event>.Filter.Eq(e => e.EventID, id);
                await _context.Events.ReplaceOneAsync(filter, ev);
                return RedirectToAction(nameof(Index));
            }
            return View(ev);
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.Find(e => e.EventID == id).FirstOrDefaultAsync();
            if (ev == null) return NotFound();

            return View(ev);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var filter = Builders<Event>.Filter.Eq(e => e.EventID, id);
            await _context.Events.DeleteOneAsync(filter);

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Find(e => e.EventID == id).Any();
        }
    }
}
