using System.IO;
using System.Text;
using ClosedXML.Excel;
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
        public async Task<IActionResult> Export(int? eventId)
        {
            var votesQuery = _context.Votes
                .Include(v => v.Event)
                .Include(v => v.FromCountry)
                .Include(v => v.ToParticipation)
                    .ThenInclude(p => p.Country)
                .AsQueryable();

            string fileNamePrefix = "AllVotes";

            if (eventId.HasValue)
            {
                votesQuery = votesQuery.Where(v => v.EventId == eventId.Value);
                var currentEvent = await _context.Events.FindAsync(eventId.Value);
                if (currentEvent != null)
                {
                    // Прибираємо пробіли для гарної назви файлу
                    fileNamePrefix = currentEvent.Name.Replace(" ", "_");
                }
            }

            var votes = await votesQuery.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Votes");
                var currentRow = 1;

                // Заголовки (робимо їх жирними)
                worksheet.Cell(currentRow, 1).Value = "Event";
                worksheet.Cell(currentRow, 2).Value = "From Country";
                worksheet.Cell(currentRow, 3).Value = "To Country";
                worksheet.Cell(currentRow, 4).Value = "Points";
                worksheet.Cell(currentRow, 5).Value = "Is Jury (Jury/Televote)";
                worksheet.Range(1, 1, 1, 5).Style.Font.Bold = true;

                // Дані
                foreach (var vote in votes)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = vote.Event?.Name;
                    worksheet.Cell(currentRow, 2).Value = vote.FromCountry?.Name;
                    worksheet.Cell(currentRow, 3).Value = vote.ToParticipation?.Country?.Name;
                    worksheet.Cell(currentRow, 4).Value = vote.Points;
                    worksheet.Cell(currentRow, 5).Value = vote.IsJury ? "Jury" : "Televote";
                }

                // Авто-ширина колонок
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"{fileNamePrefix}_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public IActionResult DownloadTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Template");

                worksheet.Cell(1, 1).Value = "Event Name";
                worksheet.Cell(1, 2).Value = "From Country";
                worksheet.Cell(1, 3).Value = "To Country";
                worksheet.Cell(1, 4).Value = "Points";
                worksheet.Cell(1, 5).Value = "Is Jury (True/False)";

                worksheet.Range(1, 1, 1, 5).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ImportTemplate.xlsx");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile importFile)
        {
            if (importFile == null || importFile.Length == 0)
            {
                return RedirectToAction(nameof(Index)); // Можна додати сповіщення про пустий файл
            }

            var errorLog = new StringBuilder();
            int addedCount = 0;

            using (var stream = new MemoryStream())
            {
                await importFile.CopyToAsync(stream);
                try
                {
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Пропускаємо перший рядок (заголовки)

                        foreach (var row in rows)
                        {
                            int rowNum = row.RowNumber();
                            try
                            {
                                string eventName = row.Cell(1).GetString().Trim();
                                string fromCountryName = row.Cell(2).GetString().Trim();
                                string toCountryName = row.Cell(3).GetString().Trim();
                                string pointsStr = row.Cell(4).GetString().Trim();
                                string isJuryStr = row.Cell(5).GetString().Trim();

                                // 1. Пошук Івенту
                                var ev = await _context.Events.FirstOrDefaultAsync(e => e.Name == eventName);
                                if (ev == null) { errorLog.AppendLine($"Row {rowNum}: Event '{eventName}' not found in database."); continue; }

                                // 2. Пошук From Country
                                var fromCountry = await _context.Countries.FirstOrDefaultAsync(c => c.Name == fromCountryName);
                                if (fromCountry == null) { errorLog.AppendLine($"Row {rowNum}: 'From Country' '{fromCountryName}' not found."); continue; }

                                // 3. Пошук To Country та перевірка її участі (Participation)
                                var toCountry = await _context.Countries.FirstOrDefaultAsync(c => c.Name == toCountryName);
                                if (toCountry == null) { errorLog.AppendLine($"Row {rowNum}: 'To Country' '{toCountryName}' not found."); continue; }

                                if (fromCountryName == toCountryName) { errorLog.AppendLine($"Row {rowNum}: Country cannot give points to itself."); continue; }

                                var participation = await _context.Participations.FirstOrDefaultAsync(p => p.EventId == ev.Id && p.CountryId == toCountry.Id);
                                if (participation == null) { errorLog.AppendLine($"Row {rowNum}: '{toCountryName}' did not participate in '{eventName}'."); continue; }

                                // 4. Валідація Points
                                if (!int.TryParse(pointsStr, out int points) || points < 1 || points > 12 || points == 11 || points == 9)
                                {
                                    errorLog.AppendLine($"Row {rowNum}: Invalid points '{pointsStr}'. Must be 1-8 or 10 or 12."); continue;
                                }

                                // 5. Парсинг IsJury
                                bool isJury = isJuryStr.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                                              isJuryStr.Equals("Jury", StringComparison.OrdinalIgnoreCase) ||
                                              isJuryStr == "1";

                                // 6. Перевірка на дублікати (як у твоєму Create)
                                var existingVote = await _context.Votes.AnyAsync(v =>
                                    v.FromCountryId == fromCountry.Id &&
                                    v.ToParticipationId == participation.Id &&
                                    v.IsJury == isJury &&
                                    v.EventId == ev.Id);

                                var isDuplicatePoints = await _context.Votes.AnyAsync(v =>
                                    v.FromCountryId == fromCountry.Id &&
                                    v.Points == points &&
                                    v.IsJury == isJury &&
                                    v.EventId == ev.Id);

                                if (existingVote) { errorLog.AppendLine($"Row {rowNum}: Vote from {fromCountryName} to {toCountryName} already exists."); continue; }
                                if (isDuplicatePoints) { errorLog.AppendLine($"Row {rowNum}: {fromCountryName} already gave {points} points in this category."); continue; }

                                // Якщо все добре — додаємо
                                var vote = new Vote
                                {
                                    EventId = ev.Id,
                                    FromCountryId = fromCountry.Id,
                                    ToParticipationId = participation.Id,
                                    Points = points,
                                    IsJury = isJury
                                };
                                _context.Votes.Add(vote);
                                addedCount++;
                            }
                            catch (Exception ex)
                            {
                                errorLog.AppendLine($"Row {rowNum}: Unexpected error - {ex.Message}");
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    errorLog.AppendLine($"File reading error: {ex.Message}. Make sure it's a valid Excel format.");
                }
            }

            // Якщо є помилки, повертаємо файл з логами
            if (errorLog.Length > 0)
            {
                string logHeader = $"Import Results:\nSuccessfully added: {addedCount} votes.\n\nErrors encountered:\n-------------------\n";
                byte[] fileBytes = Encoding.UTF8.GetBytes(logHeader + errorLog.ToString());
                string logFileName = $"ImportErrors_Log_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}.txt";
                return File(fileBytes, "text/plain", logFileName);
            }

            // Якщо все ідеально, просто повертаємо на сторінку
            return RedirectToAction(nameof(Index));
        }
    }
}
