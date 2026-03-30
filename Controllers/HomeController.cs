using EurovisionHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
            ViewBag.JuryPoints = await _context.Votes.Where(v => v.IsJury).SumAsync(v => v.Points);
            ViewBag.TelePoints = await _context.Votes.Where(v => !v.IsJury).SumAsync(v => v.Points);

            return View();
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
