using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EWP_API_WEB_APP.Data;
using EWP_API_WEB_APP.Models.Data;

namespace EWP_API_WEB_APP.Controllers
{
    public class WristbandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WristbandsController> _logger;

        public WristbandsController(ApplicationDbContext context, ILogger<WristbandsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Página inicial das bands
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("WristbandsController: Index");
            var applicationDbContext = _context.Wristbands.Include(w => w.User);
            return View(await applicationDbContext.ToListAsync());
        }
    }
}

/*
// GET: Wristbands/Details/5
public async Task<IActionResult> Details(int? id)
{
    if (id == null || _context.Wristbands == null)
    {
        return NotFound();
    }

    var wristbands = await _context.Wristbands
        .Include(w => w.User)
        .FirstOrDefaultAsync(m => m.Id == id);
    if (wristbands == null)
    {
        return NotFound();
    }

    return View(wristbands);
}

// GET: Wristbands/Create
public IActionResult Create()
{
    ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Id");
    return View();
}

// POST: Wristbands/Create
// To protect from overposting attacks, enable the specific properties you want to bind to.
// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("Id,CodeNFC,Color,ConnectedDate,Brand,Status,UserFK")] Wristbands wristbands)
{
    if (ModelState.IsValid)
    {
        _context.Add(wristbands);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Id", wristbands.UserFK);
    return View(wristbands);
}

// GET: Wristbands/Edit/5
public async Task<IActionResult> Edit(int? id)
{
    if (id == null || _context.Wristbands == null)
    {
        return NotFound();
    }

    var wristbands = await _context.Wristbands.FindAsync(id);
    if (wristbands == null)
    {
        return NotFound();
    }
    ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Id", wristbands.UserFK);
    return View(wristbands);
}

// POST: Wristbands/Edit/5
// To protect from overposting attacks, enable the specific properties you want to bind to.
// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("Id,CodeNFC,Color,ConnectedDate,Brand,Status,UserFK")] Wristbands wristbands)
{
    if (id != wristbands.Id)
    {
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        try
        {
            _context.Update(wristbands);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WristbandsExists(wristbands.Id))
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
    ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Id", wristbands.UserFK);
    return View(wristbands);
}

// GET: Wristbands/Delete/5
public async Task<IActionResult> Delete(int? id)
{
    if (id == null || _context.Wristbands == null)
    {
        return NotFound();
    }

    var wristbands = await _context.Wristbands
        .Include(w => w.User)
        .FirstOrDefaultAsync(m => m.Id == id);
    if (wristbands == null)
    {
        return NotFound();
    }

    return View(wristbands);
}

// POST: Wristbands/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    if (_context.Wristbands == null)
    {
        return Problem("Entity set 'ApplicationDbContext.Wristbands'  is null.");
    }
    var wristbands = await _context.Wristbands.FindAsync(id);
    if (wristbands != null)
    {
        _context.Wristbands.Remove(wristbands);
    }

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}

private bool WristbandsExists(int id)
{
  return (_context.Wristbands?.Any(e => e.Id == id)).GetValueOrDefault();
}
}
}
*/