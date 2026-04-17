using EurovisionHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EurovisionHub.Controllers
{
    public class EventsController : Controller
    {
        private readonly EurovisionContext _context;

        public EventsController(EurovisionContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var eurovisionContext = _context.Events.Include(p => p.Type).Include(p => p.HostCountry).OrderByDescending(e => e.Date);
            return View(await eurovisionContext.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(p => p.Type)
                .Include(p => p.HostCountry)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public IActionResult Create()
        {
            ViewData["Type"] = new SelectList(_context.EventTypes, "Id", "Name");
            ViewData["HostCountry"] = new SelectList(_context.Countries, "Id", "Name");
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Date,TypeId,HostCountryId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.Name.ToLower().Replace(" ", "") == @event.Name.ToLower().Replace(" ", "") && e.TypeId == @event.TypeId);

                if (existingEvent != null)
                {
                    if(existingEvent.Name.ToLower().Replace(" ", "") == @event.Name.ToLower().Replace(" ", ""))
                        ModelState.AddModelError("Name", "An event with the same Name already exists.");
                    if (existingEvent.TypeId == @event.TypeId)
                        ModelState.AddModelError("TypeId", "An event with the same Type already exists.");
                    ViewData["Type"] = new SelectList(_context.EventTypes, "Id", "Name", @event.TypeId);
                    ViewData["HostCountry"] = new SelectList(_context.Countries, "Id", "Name", @event.HostCountryId);
                    return View(@event);
                }
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Type"] = new SelectList(_context.EventTypes, "Id", "Name", @event.TypeId);
            ViewData["HostCountry"] = new SelectList(_context.Countries, "Id", "Name", @event.HostCountryId);
            return View(@event);
        }

        // GET: Events/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewData["Type"] = new SelectList(_context.EventTypes, "Id", "Name", @event.TypeId);
            ViewData["HostCountry"] = new SelectList(_context.Countries, "Id", "Name", @event.HostCountryId);
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Date,TypeId,HostCountryId")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Додати перевірку на унікальність назви та типу для інших записів, окрім поточного
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
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
            ViewData["Type"] = new SelectList(_context.EventTypes, "Id", "Name", @event.TypeId);
            ViewData["HostCountry"] = new SelectList(_context.Countries, "Id", "Name", @event.HostCountryId);
            return View(@event);
        }

        // GET: Events/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Type)
                .Include(p => p.HostCountry)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
