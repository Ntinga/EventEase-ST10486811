using Azure.Storage.Blobs;
using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly MongoDbContext _context;
        private readonly BlobServiceClient _blobServiceClient;

        public VenuesController(MongoDbContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venues.Find(Builders<Venue>.Filter.Empty).ToListAsync();
            return View(venues);
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.Find(v => v.VenueID == id).FirstOrDefaultAsync();

            if (venue == null) return NotFound();

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueID,Name,Location,Capacity")] Venue venue, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("venue-images");
                await containerClient.CreateIfNotExistsAsync();

                string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imageFile.FileName);
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = imageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                venue.ImageURL = blobClient.Uri.ToString();
            }

            if (ModelState.IsValid)
            {
                await _context.Venues.InsertOneAsync(venue);
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.Find(v => v.VenueID == id).FirstOrDefaultAsync();
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueID,Name,Location,Capacity,ImageURL")] Venue venue)
        {
            if (id != venue.VenueID) return NotFound();

            if (ModelState.IsValid)
            {
                var filter = Builders<Venue>.Filter.Eq(v => v.VenueID, id);
                await _context.Venues.ReplaceOneAsync(filter, venue);
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.Find(v => v.VenueID == id).FirstOrDefaultAsync();

            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool hasBookings = await _context.Bookings.Find(b => b.VenueID == id).AnyAsync();

            if (hasBookings)
            {
                TempData["AlertMessage"] = "Restriction: This venue cannot be deleted because it is associated with active or past bookings.";
                return RedirectToAction(nameof(Index));
            }

            var filter = Builders<Venue>.Filter.Eq(v => v.VenueID, id);
            await _context.Venues.DeleteOneAsync(filter);

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Find(v => v.VenueID == id).Any();
        }
    }
}
