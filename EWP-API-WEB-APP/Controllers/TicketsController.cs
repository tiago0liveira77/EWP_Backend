using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EWP_API_WEB_APP.Data;
using EWP_API_WEB_APP.Models.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EWP_API_WEB_APP.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketsController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TicketsController(ApplicationDbContext context, UserManager<Users> userManager, RoleManager<IdentityRole> roleManager, ILogger<TicketsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Ver bilhetes de um só evento
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isMyTickets"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int? id, bool? isMyTickets)
        {
            _logger.LogInformation("TicketsController: Index");
            if (isMyTickets == null)
            {
                isMyTickets = false;
            }

            //Obter lista de todos os bilhetes
            var ticketsQuery = _context.Tickets
                .Include(t => t.ListaUtilizadores)
                .Include(t => t.Event)
                .ThenInclude(e => e.ListaUtilizadores)
                .Where(t => t.EventFK == id);

            if (isMyTickets == true)
            {
                // Obter lista de bilhetes de um só utilizador
                var user = await _userManager.GetUserAsync(User);
                ticketsQuery = _context.Tickets
                    .Include(t => t.ListaUtilizadores)
                    .Include(t => t.Event)
                    .Where(t => t.ListaUtilizadores.Contains(_context.Users.Find(user.Id)));
            }

            var tickets = await ticketsQuery.ToListAsync();

            //Obtem o nome do evento para apresentar
            var eventName = _context.Events.FirstOrDefault(e => e.Id == id)?.Name;
            ViewData["eventName"] = eventName;

            var eventBelongsToUser = _context.Events.Any(e => e.Id == id && e.ListaUtilizadores.Any(u => u.UserName == User.Identity.Name));
            ViewData["showCreateButton"] = User.Identity.IsAuthenticated && eventBelongsToUser;

            return View(tickets);
        }

        /// <summary>
        /// Verifica os detalhes de um bilhete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            _logger.LogInformation("TicketsController: Details");
            if (id == null || _context.Tickets == null)
            {
                _logger.LogWarning("TicketsController: Details - Invalid ticket id or null context");
                return NotFound();
            }

            //Obtem a informação do bilehte escolhido
            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tickets == null)
            {
                _logger.LogWarning("TicketsController: Details - Ticket not found");
                return NotFound();
            }

            return View(tickets);
        }

        /// <summary>
        /// Página de criar bilhetes
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("TicketsController: Create");
            //Obter utilizador
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userId = user.Id;
                var userFromContext = _context.Users.Find(userId);
                _logger.LogWarning("TicketsController: Create - User: " + userFromContext.Name);
                //Criar novo evento
                var events = _context.Events.Where(e => e.ListaUtilizadores.Contains(userFromContext)).ToList();

                ViewData["EventFK"] = new SelectList(events, "Id", "Name");
                ViewData["EventID"] = events.First().Id;
            }
            return View();
        }

        /// <summary>
        /// Cria os bilhetes
        /// </summary>
        /// <param name="tickets"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Nome,Description,Price,Selling,EventFK")] Tickets tickets)
        {
            _logger.LogInformation("TicketsController: Create [HttpPost]");
            if (ModelState.IsValid)
            {
                _context.Add(tickets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = tickets.EventFK });
            }
            ViewData["EventFK"] = new SelectList(_context.Events, "Id", "Id", tickets.EventFK);
            return View();
        }

        /// <summary>
        /// Página para editar bilhetes
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            _logger.LogInformation("TicketsController: Edit");
            if (id == null || _context.Tickets == null)
            {
                _logger.LogWarning("TicketsController: Edit - Invalid ticket id or null context");
                return NotFound();
            }

            var tickets = await _context.Tickets.FindAsync(id);
            if (tickets == null)
            {
                _logger.LogWarning("TicketsController: Edit - Ticket not found");
                return NotFound();
            }


            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {

                var events = _context.Events.Where(e => e.ListaUtilizadores.Any(u => u.Id == user.Id)).ToList();

                ViewData["EventFK"] = new SelectList(events, "Id", "Name");
                ViewData["EventID"] = events.FirstOrDefault()?.Id;
            }

            return View(tickets);
        }

        /// <summary>
        /// Ação de editar um bilhete
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tickets"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Description,Price,Selling,EventFK")] Tickets tickets)
        {
            _logger.LogInformation("TicketsController: Edit [HttpPost]");
            if (id != tickets.Id)
            {
                _logger.LogWarning("TicketsController: Edit - Invalid ticket id");
                return NotFound();
            }

            //Valida se o model está válido
            if (ModelState.IsValid)
            {
                try
                {
                    //Dá update ao ticket com as respetivas informações
                    _context.Update(tickets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketsExists(tickets.Id))
                    {
                        _logger.LogWarning("TicketsController: Edit - Ticket not found");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = tickets.EventFK });
            }
            //Obter utilizador
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {

                var events = _context.Events.Where(e => e.ListaUtilizadores.Any(u => u.Id == user.Id)).ToList();

                ViewData["EventFK"] = new SelectList(events, "Id", "Name");
                ViewData["EventID"] = events.FirstOrDefault()?.Id;
            }
            return View(tickets);
        }

        /// <summary>
        /// Página para eliminar um bilhete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            _logger.LogInformation("TicketsController: Delete");
            if (id == null || _context.Tickets == null)
            {
                _logger.LogWarning("TicketsController: Delete - Invalid ticket id or null context");
                return NotFound();
            }

            //Obtem a informação do bilhete para eliminar
            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tickets == null)
            {
                _logger.LogWarning("TicketsController: Delete - Ticket not found");
                return NotFound();
            }

            return View(tickets);
        }

        /// <summary>
        /// Ação de eliminar bilhete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("TicketsController: DeleteConfirmed");
            if (_context.Tickets == null)
            {
                _logger.LogWarning("TicketsController: DeleteConfirmed - Null context");
                return Problem("Entity set 'ApplicationDbContext.Tickets' is null.");
            }
            var tickets = await _context.Tickets.FindAsync(id);
            if (tickets != null)
            {
                _context.Tickets.Remove(tickets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = tickets.EventFK });
        }

        /// <summary>
        /// Ação de comprar um bilhete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> BuyTicket(int? id)
        {
            _logger.LogInformation("TicketsController: BuyTicket");
            if (id == null)
            {
                _logger.LogWarning("TicketsController: BuyTicket - Invalid ticket id");
                return NotFound();
            }
            //Obtem o bilhete escolhido
            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null)
            {
                _logger.LogWarning("TicketsController: BuyTicket - Ticket not found");
                return NotFound();
            }

            // Decrementa a quantidade de bilhetes disponiveis
            if (ticket.Selling > 0)
            {
                ticket.Selling--;
                await _context.SaveChangesAsync(); // Guardar alterações
            }
            else
            {
                //Não tem bilhetes disponiveis
                return RedirectToAction(nameof(Index));
            }

            // Associa o bilhete com o utilizador
            var user = await _userManager.GetUserAsync(User);
            user.ListaBilhetes.Add(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = ticket.EventFK });
        }

        private bool TicketsExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
