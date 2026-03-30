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
    public class VotesController : Controller
    {
        private readonly EurovisionContext _context;

        public VotesController(EurovisionContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? eventId)
        {
            var viewModel = new VotingViewModel
            {
                SelectedEventId = eventId,
                Events = await _context.Events.ToListAsync(),
                Votes = new List<Vote>()
            };

            if (eventId.HasValue)
            {
                var currentEvent = viewModel.Events.FirstOrDefault(e => e.Id == eventId.Value);
                viewModel.SelectedEventName = currentEvent != null ? $"{currentEvent.Name} - {currentEvent.Date.Value.Year}" : "";

                viewModel.Votes = await _context.Votes
                    .Include(v => v.FromCountry)
                    .Include(v => v.ToParticipation)
                        .ThenInclude(tp => tp.Country)
                    .Include(v => v.Event)
                    .Where(v => v.ToParticipation.EventId == eventId.Value)
                    .ToListAsync();
            }

            return View(viewModel);
        }

        // GET: Votes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vote = await _context.Votes
                .Include(v => v.FromCountry)
                .Include(v => v.ToParticipation)
                    .ThenInclude(tp => tp.Country)
                .Include(v => v.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vote == null)
            {
                return NotFound();
            }
            ViewBag.SelectedEventId = vote.EventId;

            return View(vote);
        }

        // GET: Votes/Create
        public IActionResult Create(int? selectedEventId)
        {
            if (selectedEventId == null) return RedirectToAction("Index");

            var participations = _context.Participations
                .Where(p => p.EventId == selectedEventId)
                .Include(p => p.Country)
                .Include(p => p.Song)
                .ToList();

            var fromCountries = participations
                .Select(p => p.Country)
                .Distinct()
                .OrderBy(c => c.Name);

            var toParticipations = participations.Select(p => new {
                p.Id,
                DisplayName = $"{p.Country.Name} : {p.Song.Artist} - {p.Song.Title}"
            }).OrderBy(x => x.DisplayName);

            ViewData["FromCountry"] = new SelectList(fromCountries, "Id", "Name");
            ViewData["ToCountry"] = new SelectList(toParticipations, "Id", "DisplayName");
            ViewBag.SelectedEventId = selectedEventId;

            return View();
        }

        // POST: Votes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FromCountryId,ToParticipationId,Points,IsJury,EventId")] Vote vote)
        {
            if (!_context.Events.Any(e => e.Id == vote.EventId))
            {
                ModelState.AddModelError(nameof(vote.EventId), "Selected event does not exist.");
            }

            if (ModelState.IsValid)
            {
                var existingVote = _context.Votes
                    .Include(c => c.FromCountry)
                    .Include(c => c.ToParticipation)
                        .ThenInclude(tp => tp.Country)
                    .FirstOrDefault(v =>
                    v.FromCountryId == vote.FromCountryId &&
                    v.ToParticipationId == vote.ToParticipationId &&
                    v.IsJury == vote.IsJury &&
                    v.EventId == vote.EventId);
                var IsDublicatePoints = _context.Votes.Any(v =>
                    v.FromCountryId == vote.FromCountryId &&
                    v.Points == vote.Points &&
                    v.IsJury == vote.IsJury &&
                    v.EventId == vote.EventId);
                if (existingVote != null || IsDublicatePoints)
                {
                    if (existingVote != null)
                    {
                        ModelState.AddModelError("ToParticipationId", $"{existingVote.ToParticipation.Country.Name} already have points from {existingVote.FromCountry.Name}.");
                        ModelState.AddModelError("FromCountryId", " ");
                    }
                    else
                    {
                        ModelState.AddModelError("FromCountryId", $"{_context.Countries.FirstOrDefault(c => c.Id == vote.FromCountryId).Name} already gives {vote.Points} points to another participant.");
                        ModelState.AddModelError("Points", " ");
                    }
                    var _participations = _context.Participations
                        .Where(p => p.EventId == vote.EventId)
                        .Include(p => p.Country)
                        .Include(p => p.Song)
                        .ToList();

                    var _fromCountries = _participations
                        .Select(p => p.Country)
                        .Distinct()
                        .OrderBy(c => c.Name);

                    var _toParticipations = _participations.Select(p => new {
                        p.Id,
                        DisplayName = $"{p.Country.Name} : {p.Song.Artist} - {p.Song.Title}"
                    }).OrderBy(x => x.DisplayName);

                    ViewData["FromCountry"] = new SelectList(_fromCountries, "Id", "Name");
                    ViewData["ToCountry"] = new SelectList(_toParticipations, "Id", "DisplayName");
                    ViewBag.SelectedEventId = vote.EventId;

                    return View(vote);
                }

                _context.Add(vote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { eventId = vote.EventId });
            }
            var participations = _context.Participations
                .Where(p => p.EventId == vote.EventId)
                .Include(p => p.Country)
                .Include(p => p.Song)
                .ToList();

            var fromCountries = participations
                .Select(p => p.Country)
                .Distinct()
                .OrderBy(c => c.Name);

            var toParticipations = participations.Select(p => new {
                p.Id,
                DisplayName = $"{p.Country.Name} : {p.Song.Artist} - {p.Song.Title}"
            }).OrderBy(x => x.DisplayName);

            ViewData["FromCountry"] = new SelectList(fromCountries, "Id", "Name");
            ViewData["ToCountry"] = new SelectList(toParticipations, "Id", "DisplayName");
            ViewBag.SelectedEventId = vote.EventId;

            return View(vote);
        }

        // GET: Votes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vote = await _context.Votes.FindAsync(id);
            if (vote == null)
            {
                return NotFound();
            }
            var participations = _context.Participations
                .Where(p => p.EventId == vote.EventId)
                .Include(p => p.Country)
                .Include(p => p.Song)
                .ToList();

            var fromCountries = participations
                .Select(p => p.Country)
                .Distinct()
                .OrderBy(c => c.Name);

            var toParticipations = participations.Select(p => new {
                p.Id,
                DisplayName = $"{p.Country.Name} : {p.Song.Artist} - {p.Song.Title}"
            }).OrderBy(x => x.DisplayName);

            ViewData["FromCountry"] = new SelectList(fromCountries, "Id", "Name");
            ViewData["ToCountry"] = new SelectList(toParticipations, "Id", "DisplayName");
            ViewBag.SelectedEventId = vote.EventId;
            return View(vote);
        }

        // POST: Votes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FromCountryId,ToParticipationId,Points,IsJury,EventId")] Vote vote)
        {
            if (id != vote.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoteExists(vote.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { eventId = vote.EventId });
            }
            var participations = _context.Participations
                .Where(p => p.EventId == vote.EventId)
                .Include(p => p.Country)
                .Include(p => p.Song)
                .ToList();

            var fromCountries = participations
                .Select(p => p.Country)
                .Distinct()
                .OrderBy(c => c.Name);

            var toParticipations = participations.Select(p => new {
                p.Id,
                DisplayName = $"{p.Country.Name} : {p.Song.Artist} - {p.Song.Title}"
            }).OrderBy(x => x.DisplayName);

            ViewData["FromCountry"] = new SelectList(fromCountries, "Id", "Name");
            ViewData["ToCountry"] = new SelectList(toParticipations, "Id", "DisplayName");
            ViewBag.SelectedEventId = vote.EventId;

            return View(vote);
        }

        // GET: Votes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vote = await _context.Votes
                .Include(v => v.FromCountry)
                .Include(v => v.ToParticipation)
                    .ThenInclude(tp => tp.Country)
                .Include(v => v.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vote == null)
            {
                return NotFound();
            }
            ViewBag.SelectedEventId = vote.EventId;

            return View(vote);
        }

        // POST: Votes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vote = await _context.Votes.FindAsync(id);
            if (vote == null)
            {
                return NotFound();
            }

            _context.Votes.Remove(vote);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { eventId = vote.EventId });
        }

        private bool VoteExists(int id)
        {
            return _context.Votes.Any(e => e.Id == id);
        }
        /*public IActionResult VotingPage(int? eventId)
        {
            var viewModel = new VotingViewModel();
            viewModel.Events = _context.Events.ToList(); // Завантажуємо список для вибору

            if (eventId.HasValue)
            {
                // Логіка, якщо захід вже обрано
                viewModel.SelectedEventId = eventId.Value;
                viewModel.Participations = _context.Participations
                                          .Where(p => p.EventId == eventId.Value)
                                          .ToList();
            }

            return View(viewModel);
        }*/
    }
}
