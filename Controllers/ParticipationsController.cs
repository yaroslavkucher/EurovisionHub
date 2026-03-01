using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EurovisionHub.Models;

namespace EurovisionHub.Controllers
{
    public class ParticipationsController : Controller
    {
        private readonly EurovisionContext _context;

        public ParticipationsController(EurovisionContext context)
        {
            _context = context;
        }

        // GET: Participations
        public async Task<IActionResult> Index()
        {
            var eurovisionContext = _context.Participations.Include(p => p.Country).Include(p => p.Event).Include(p => p.Song);
            return View(await eurovisionContext.ToListAsync());
        }

        // GET: Participations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participation = await _context.Participations
                .Include(p => p.Country)
                .Include(p => p.Event)
                .Include(p => p.Song)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participation == null)
            {
                return NotFound();
            }

            return View(participation);
        }

        // GET: Participations/Create
        public IActionResult Create()
        {
            ViewData["Country"] = new SelectList(_context.Countries, "Id", "Name");
            ViewData["Event"] = new SelectList(_context.Events, "Id", "Name");
            ViewData["Song"] = new SelectList(_context.Songs, "Id", "Title");
            return View();
        }

        // POST: Participations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CountryId,SongId,EventId,OrderNumber")] Participation participation)
        {
            if (ModelState.IsValid)
            {
                var _participation = await _context.Participations.FirstOrDefaultAsync(p => p.CountryId == participation.CountryId && p.EventId == participation.EventId && p.SongId == participation.SongId);
                if (_participation == null) {
                _context.Add(participation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
                    }
                 ModelState.AddModelError(string.Empty, "This participation already exists.");
            }
            ViewData["Country"] = new SelectList(_context.Countries, "Id", "Name");
            ViewData["Event"] = new SelectList(_context.Events, "Id", "Name");
            ViewData["Song"] = new SelectList(_context.Songs, "Id", "Title");
            return View(participation);
        }

        // GET: Participations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participation = await _context.Participations.FindAsync(id);
            if (participation == null)
            {
                return NotFound();
            }
            ViewData["Country"] = new SelectList(_context.Countries, "Id", "Name", participation.CountryId);
            ViewData["Event"] = new SelectList(_context.Events, "Id", "Name", participation.EventId);
            ViewData["Song"] = new SelectList(_context.Songs, "Id", "Title", participation.SongId);
            return View(participation);
        }

        // POST: Participations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CountryId,SongId,EventId,OrderNumber")] Participation participation)
        {
            if (id != participation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ParticipationExists(participation))
                    {
                        ModelState.AddModelError(string.Empty, "This participation already exists.");
                        ViewData["Country"] = new SelectList(_context.Countries, "Id", "Name");
                        ViewData["Event"] = new SelectList(_context.Events, "Id", "Name");
                        ViewData["Song"] = new SelectList(_context.Songs, "Id", "Title");
                        return View(participation);
                    }
                    _context.Update(participation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParticipationExists(participation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Country"] = new SelectList(_context.Countries, "Id", "Name", participation.CountryId);
            ViewData["Event"] = new SelectList(_context.Events, "Id", "Name", participation.EventId);
            ViewData["Song"] = new SelectList(_context.Songs, "Id", "Title", participation.SongId);
            return View(participation);
        }

        // GET: Participations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participation = await _context.Participations
                .Include(p => p.Country)
                .Include(p => p.Event)
                .Include(p => p.Song)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participation == null)
            {
                return NotFound();
            }

            return View(participation);
        }

        // POST: Participations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participation = await _context.Participations.FindAsync(id);
            if (participation != null)
            {
                _context.Participations.Remove(participation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParticipationExists(Participation participation)
        {
            return _context.Participations.Any(p => p.CountryId == participation.CountryId && p.EventId == participation.EventId && p.SongId == participation.SongId);
        }
        private bool ParticipationExists(int id)
        {
            return _context.Participations.Any(p => p.Id == id);
        }
    }
}
