using EventEase.Data;
using EventEase.Models;
using EventEase.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly MongoDbContext _context;

        public BookingsController(MongoDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string searchString)
        {
            var bookings = await _context.Bookings.Find(Builders<Booking>.Filter.Empty).ToListAsync();
            var events = await _context.Events.Find(Builders<Event>.Filter.Empty).ToListAsync();
            var venues = await _context.Venues.Find(Builders<Venue>.Filter.Empty).ToListAsync();

            var bookingVMs = bookings.Select(b => new BookingViewModel
            {
                BookingID = b.BookingID,
                EventName = events.FirstOrDefault(e => e.EventID == b.EventID)?.EventName ?? "Unknown Event",
                VenueName = venues.FirstOrDefault(v => v.VenueID == b.VenueID)?.Name ?? "Unknown Venue",
                StartDate = b.StartDate,
                EndDate = b.EndDate
            }).ToList();

            return View(bookingVMs);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.Find(b => b.BookingID == id).FirstOrDefaultAsync();
            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventID"] = new SelectList(_context.Events.Find(Builders<Event>.Filter.Empty).ToList(), "EventID", "EventID");
            ViewData["VenueID"] = new SelectList(_context.Venues.Find(Builders<Venue>.Filter.Empty).ToList(), "VenueID", "VenueID");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,VenueID,EventID,StartDate,EndDate")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                bool isBooked = await _context.Bookings.Find(b =>
                    b.VenueID == booking.VenueID &&
                    booking.StartDate < b.EndDate &&
                    booking.EndDate > b.StartDate).AnyAsync();

                if (isBooked)
                {
                    ModelState.AddModelError("", "The selected venue is already booked during this time period.");
                    ViewData["EventID"] = new SelectList(_context.Events.Find(Builders<Event>.Filter.Empty).ToList(), "EventID", "EventName", booking.EventID);
                    ViewData["VenueID"] = new SelectList(_context.Venues.Find(Builders<Venue>.Filter.Empty).ToList(), "VenueID", "Name", booking.VenueID);
                    return View(booking);
                }

                if (booking.EndDate <= booking.StartDate)
                {
                    ModelState.AddModelError("EndDate", "The End Date must be after the Start Date.");
                    ViewData["EventID"] = new SelectList(_context.Events.Find(Builders<Event>.Filter.Empty).ToList(), "EventID", "EventName", booking.EventID);
                    ViewData["VenueID"] = new SelectList(_context.Venues.Find(Builders<Venue>.Filter.Empty).ToList(), "VenueID", "Name", booking.VenueID);
                    return View(booking);
                }

                await _context.Bookings.InsertOneAsync(booking);
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventID"] = new SelectList(_context.Events.Find(Builders<Event>.Filter.Empty).ToList(), "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venues.Find(Builders<Venue>.Filter.Empty).ToList(), "VenueID", "Name", booking.VenueID);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.Find(b => b.BookingID == id).FirstOrDefaultAsync();
            if (booking == null) return NotFound();

            ViewData["EventID"] = new SelectList(_context.Events.Find(Builders<Event>.Filter.Empty).ToList(), "EventID", "EventID", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venues.Find(Builders<Venue>.Filter.Empty).ToList(), "VenueID", "VenueID", booking.VenueID);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingID,VenueID,EventID,StartDate,EndDate")] Booking booking)
        {
            if (id != booking.BookingID) return NotFound();

            if (ModelState.IsValid)
            {
                var filter = Builders<Booking>.Filter.Eq(b => b.BookingID, id);
                await _context.Bookings.ReplaceOneAsync(filter, booking);
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventID"] = new SelectList(_context.Events.Find(Builders<Event>.Filter.Empty).ToList(), "EventID", "EventID", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venues.Find(Builders<Venue>.Filter.Empty).ToList(), "VenueID", "VenueID", booking.VenueID);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.Find(b => b.BookingID == id).FirstOrDefaultAsync();
            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var filter = Builders<Booking>.Filter.Eq(b => b.BookingID, id);
            await _context.Bookings.DeleteOneAsync(filter);
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Find(b => b.BookingID == id).Any();
        }
    }
}
