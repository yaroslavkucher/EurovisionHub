using EurovisionHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace EurovisionHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly EurovisionContext _context;

        public HomeController(EurovisionContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var latestFinal = await _context.Events
        .Include(e => e.Type)
        .Where(e => e.Type.Name == "Final" && e.Date < DateTime.UtcNow)
        .OrderByDescending(e => e.Date)
        .FirstOrDefaultAsync();

            if (latestFinal == null) return View();

            ViewBag.EventId = latestFinal.Id;
            ViewBag.EventName = latestFinal.Name;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool hasVoted = false;

            if (userId != null)
            {
                hasVoted = await _context.WinnerOpinions
                    .AnyAsync(o => o.UserId == userId && o.EventId == latestFinal.Id);
            }

            ViewBag.HasVoted = hasVoted;

            if (hasVoted)
            {
                var results = await GetPollResults(latestFinal.Id);
                ViewBag.AgreeCount = results.Agree;
                ViewBag.DisagreeCount = results.Disagree;
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Vote(int eventId, bool agree)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var alreadyVoted = await _context.WinnerOpinions
                .AnyAsync(o => o.UserId == userId && o.EventId == eventId);

            if (alreadyVoted) return BadRequest("You have already voted.");

            var opinion = new WinnerOpinion
            {
                UserId = userId,
                EventId = eventId,
                IsAgree = agree
            };

            _context.WinnerOpinions.Add(opinion);
            await _context.SaveChangesAsync();

            var results = await GetPollResults(eventId);

            return Json(new { agree = results.Agree, disagree = results.Disagree });
        }

        private async Task<(int Agree, int Disagree)> GetPollResults(int eventId)
        {
            var votes = await _context.WinnerOpinions
                .Where(o => o.EventId == eventId)
                .ToListAsync();

            return (votes.Count(v => v.IsAgree), votes.Count(v => !v.IsAgree));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
