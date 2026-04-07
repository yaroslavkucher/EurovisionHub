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
    public class CountriesController : Controller
    {
        private readonly EurovisionContext _context;

        public CountriesController(EurovisionContext context)
        {
            _context = context;
        }

        // GET: Countries
        public async Task<IActionResult> Index()
        {
            // Завантажуємо країни разом із результатами та датами подій
            var countries = await _context.Countries
                .Include(c => c.Participations)
                    .ThenInclude(p => p.Event)
                .ToListAsync();

            // Готуємо дані для графіка: назва країни та список пар {рік, місце}
            var countriesHistory = countries
                .Select(c => new {
                    Name = c.Name,
                    Results = c.Participations
                        .Where(p => p.Place.HasValue && p.Event != null && p.Event.Date.HasValue)
                        .OrderBy(p => p.Event.Date.Value.Year)
                        .Select(p => new {
                            Year = p.Event.Date.Value.Year,
                            Place = p.Place.Value
                        })
                        .ToList()
                })
                .Where(c => c.Results.Any()) // Беремо тільки ті країни, що мають результати
                .ToList();

            ViewBag.CountriesHistory = countriesHistory;

            return View(countries.OrderBy(c => c.Name).ToList());
        }

        // GET: Countries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries
                .Include(c => c.Participations)
                    .ThenInclude(p => p.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (country == null)
            {
                return NotFound();
            }

            var participationsData = country.Participations
                .Where(p => p.Place.HasValue)
                .OrderBy(p => p.Event.Date.Value.Year)
                .Select(p => new {
                    Year = p.Event.Date.Value.Year,
                    Place = p.Place.Value
                })
                .ToList();
            ViewBag.Years = participationsData.Select(d => d.Year).ToList();
            ViewBag.Places = participationsData.Select(d => d.Place).ToList();

            return View(country);
        }

        // GET: Countries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Countries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,FlagUrl")] Country country)
        {
            if (ModelState.IsValid)
            {
                var existing_item = await _context.Countries.FirstOrDefaultAsync(c => c.Name.ToLower() == country.Name.ToLower() || c.Code == country.Code.ToUpper());
                if (existing_item != null)
                {
                    if (existing_item.Name.ToLower() == country.Name.ToLower())
                    {
                        ModelState.AddModelError("Name", "A country with the same name already exists.");
                    }
                    if (existing_item.Code == country.Code.ToUpper())
                    {
                        ModelState.AddModelError("Code", "A country with the same code already exists.");
                    }
                    return View(country);
                }
                country.Code = country.Code.ToUpper();
                _context.Add(country);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Country {country.Name} successfully added to database!";
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        // GET: Countries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }
            return View(country);
        }

        // POST: Countries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,FlagUrl")] Country country)
        {
            if (id != country.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                country.Code = country.Code?.ToUpper();

                bool nameExists = await _context.Countries
                    .AnyAsync(c => c.Name.ToLower() == country.Name.ToLower() && c.Id != id);

                bool codeExists = await _context.Countries
                    .AnyAsync(c => c.Code == country.Code && c.Id != id);

                if (nameExists)
                {
                    ModelState.AddModelError("Name", "Country with this Name already exists.");
                }

                if (codeExists)
                {
                    ModelState.AddModelError("Code", "Country with this Code already exists.");
                }

                if (nameExists || codeExists)
                {
                    return View(country);
                }

                try
                {
                    _context.Update(country);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CountryExists(country.Id))
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
            return View(country);
        }

        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (country == null)
            {
                return NotFound();
            }

            return View(country);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country != null)
            {
                _context.Countries.Remove(country);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Error: Unable to delete country. It may be referenced by other records.";
                return View(country);
            }
            TempData["Success"] = $"Country {country.Name} successfully deleted!";
            return RedirectToAction(nameof(Index));
        }

        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }
    }
}
