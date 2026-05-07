using Azure.Storage.Blobs;
using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly BlobServiceClient _blobServiceClient;

        public VenuesController(AppDbContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueID == id);

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
                //Gettng a container client
                var containerClient = _blobServiceClient.GetBlobContainerClient("venue-images");
                await containerClient.CreateIfNotExistsAsync();

                //Creating an unique name for the blob
                string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imageFile.FileName);
                var blobClient = containerClient.GetBlobClient(fileName);

                //Upload the file
                using (var stream = imageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                //Save the URL to the database
                venue.ImageURL = blobClient.Uri.ToString();
            }

            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FindAsync(id);
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
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueID == id);

            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool hasActiveBookings = await _context.Bookings.AnyAsync(b => b.VenueID == id && b.EndDate >= DateTime.Now);

            if (hasActiveBookings)
            {
                TempData["AlertMessage"] = "Restriction: This venue cannot be deleted because it has an active booking.";
                return RedirectToAction(nameof(Index));
            }
            var venue = await _context.Venues.FindAsync(id);

            // Check for active bookings
            bool hasBookings = _context.Bookings.Any(b => b.VenueID == id);

            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete venue because it is associated with active bookings.";
                return RedirectToAction(nameof(Index));
            }

            if (venue != null)
            {
                _context.Venues.Remove(venue);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueID == id);
        }
    }
}
