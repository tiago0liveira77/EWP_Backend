using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EWP_API_WEB_APP.Data;
using EWP_API_WEB_APP.Models.Data;
using EWP_API_WEB_APP.Utilities.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

namespace EWP_API_WEB_APP.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EventsController> _logger;
        private readonly UserManager<Users> _userManager;

        public EventsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<EventsController> logger, UserManager<Users> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Pesquisa dos eventos todos disponiveis e listagem dos mesmos
        /// Se "isMyEvents" vier a true, significa que estamos a consultar os eventos do utilizador logado
        /// </summary>
        /// <returns> Lista de eventos </returns>
        public async Task<IActionResult> Index(bool? isMyEvents)
        {
            _logger.LogInformation("Executing Index action of EventsController.");

            ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;

            if (isMyEvents == null)
                isMyEvents = false;

            //Obter eventos globais
            if (isMyEvents == false)
            {
                _logger.LogInformation("Retrieving global events.");
                var events = await _context.Events
                    .Include(e => e.Type)
                    .Include(e => e.ListaUtilizadores)
                    .Include(e => e.ListaBilhetes)
                    .ToListAsync();

                return View(events);
            }
            else
            {
                //Obter eventos do utilizador
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    _logger.LogInformation($"Retrieving events for user: {user.UserName}");
                    var events = await _context.Events
                        .Include(e => e.Type)
                        .Include(e => e.ListaUtilizadores)
                        .Include(e => e.ListaBilhetes)
                        .Where(e => e.ListaUtilizadores.Contains(_context.Users.Find(user.Id)))
                        .ToListAsync();

                    return View(events);
                }
            }

            return NotFound();
        }

        /// <summary>
        /// Mostra os detalhes de um evento
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Detalhes do evento </returns>
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            _logger.LogInformation("Executing Details action of EventsController.");

            //Valida se o parâmetro do ID foi inserido
            if (id == null || _context.Events == null)
            {
                _logger.LogWarning("Invalid or missing event ID.");
                return NotFound();
            }

            //Obtem o evento escolhido pelo utilizador e o seu tipo (tabela à parte)
            var events = await _context.Events
                .Include(e => e.Type)
                .FirstOrDefaultAsync(m => m.Id == id);

            //Valida se o evento existe
            if (events == null)
            {
                _logger.LogWarning("Event not found.");
                return NotFound();
            }

            //É criado um ViewData para poder transportar o tipo de evento para o ecrã
            ViewData["eventType"] = events.Type.Name;

            return View(events);
        }

        /// <summary>
        /// Constroi o ecrã de input de eventos
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Create()
        {
            _logger.LogInformation("Executing Create action of EventsController.");

            //Obtem lista de tipos de eventos   
            IList<string> tipoEventos = _context.EventsType.Select(record => record.Name).ToList();

            //Insere a lista dentro do ViewData para ser mapeada e mostrada ao utilizador
            ViewData["eventsType"] = tipoEventos;

            return View();
        }

        /// <summary>
        /// Método tem a função de criar um novo evento na base de dados
        /// </summary>
        /// <param name="events"></param> Objeto com toda a informação sobre o evento
        /// <param name="file"></param> Ficheiro (img) associado ao evento (teve que ser separado do objeto porque no objeto é só o nome do ficheiro)
        /// <param name="type"></param> Tipo do evento (separado do objeto porque no Model usa uma tabela externa (FK))
        /// <returns> Lista de eventos </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,Description,StartingDate,EndingDate,Location")] Events events, IFormFile Ficheiro, string type)
        {
            _logger.LogInformation("Executing Create POST action of EventsController.");

            // Ficheiro e Tipo removidos das validações do Model porque têm que ser tratados de forma independente.
            ModelState.Remove("Ficheiro");
            ModelState.Remove("Type");

            //Criado o path para dar upload
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/uploadedEventImages");
            //Upload do ficheiro
            events.Ficheiro = FileUtils.addImageToServer(Ficheiro, _logger, uploadPath);

            // Obtem o tipo escolhid pelo utilizador
            if (!string.IsNullOrEmpty(type))
            {
                _logger.LogInformation("Processing selected event type.");

                EventsType eventType = _context.EventsType.FirstOrDefault(e => e.Name == type);
                if (eventType != null)
                {
                    events.Type = eventType;
                    _logger.LogInformation($"Selected event type: {eventType.Name}");
                }
            }

            //Valida se o resto dos dados estão OK
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model state is valid. Adding new event.");

                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var userId = user.Id;
                    var userFromContext = _context.Users.Find(userId);
                    if (userFromContext != null)
                    {
                        //Criar evento
                        _logger.LogInformation("User validated. Saving new event.");

                        await _context.AddAsync(events);
                        userFromContext.ListaEventos.Add(events);
                        events.ListaUtilizadores.Add(userFromContext);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogWarning("User not found.");
                        return NotFound();
                    }
                }
                else
                {
                    _logger.LogWarning("User not found.");
                    return NotFound();
                }

                _logger.LogInformation("New event added successfully.");
                return RedirectToAction(nameof(Index));
            }

            IList<string> tipoEventos = _context.EventsType.Select(record => record.Name).ToList();
            ViewData["eventsType"] = tipoEventos;

            return View(events);
        }

        /// <summary>
        /// Constroi o ecrã de edição de evento
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Dados do evento </returns>
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            _logger.LogInformation("Executing Edit action of EventsController.");

            //Valida se o parâmetro está preenchido
            if (id == null || _context.Events == null)
            {
                _logger.LogWarning("Invalid or missing event ID.");
                return NotFound();
            }

            //Obtem os eventos com o seu respetivo tipo
            var events = await _context.Events.Include(e => e.Type).FirstOrDefaultAsync(e => e.Id == id);
            TempData["imageEvent"] = events.Ficheiro;

            //Valida se evento existe
            if (events == null)
            {
                _logger.LogWarning("Event not found.");
                return NotFound();
            }

            //Criada uma dropdown para mostrar ao utilizador
            SelectList tipoEventos = new SelectList(_context.EventsType, "Id", "Name", events.Type.Id);
            ViewBag.Eventype = tipoEventos;

            int eventType = events.Type.Id;
            ViewData["selectedType"] = eventType;

            return View(events);
        }

        /// <summary>
        /// Método para guardar as edições do utilizador, são feitas validações do ficheiro e do tipo de evento
        /// </summary>
        /// <param name="id"></param>
        /// <param name="events"></param>
        /// <param name="file"></param>
        /// <param name="EventType"></param>
        /// <returns> </returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartingDate,EndingDate,Location")] Events events, IFormFile Ficheiro, string EventType)
        {
            _logger.LogInformation("Executing Edit POST action of EventsController.");

            if (id != events.Id)
            {
                _logger.LogWarning("Invalid event ID.");
                return NotFound();
            }

            // Ficheiro e Tipo removidos das validações do Model porque têm que ser tratados de forma independente.
            ModelState.Remove("Ficheiro");
            ModelState.Remove("Type");

            events.Ficheiro = TempData["imageEvent"].ToString();

            if (Ficheiro != null)
            {
                //Criado o path para dar upload
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/uploadedEventImages");
                //Upload do ficheiro
                events.Ficheiro = FileUtils.addImageToServer(Ficheiro, _logger, uploadPath);
            }

            // Process the type
            if (!string.IsNullOrEmpty(EventType))
            {
                EventsType eType = _context.EventsType.FirstOrDefault(e => e.Id == int.Parse(EventType));
                if (eType != null)
                {
                    events.Type = eType;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(events);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventsExists(events.Id))
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

            //Criadoa uma dropdown para mostrar ao utilizador
            SelectList tipoEventos = new SelectList(_context.EventsType, "Id", "Name", events.Type.Id);
            ViewBag.Eventype = tipoEventos;

            int eventType = events.Type.Id;
            ViewData["selectedType"] = eventType;

            return View(events);
        }

        /// <summary>
        /// Cria o ecrã de delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            _logger.LogInformation("Executing Delete action of EventsController.");

            //Valida se o parâmetro está preenchido
            if (id == null || _context.Events == null)
            {
                _logger.LogWarning("Invalid or missing event ID.");
                return NotFound();
            }

            //Obtem os eventos com o seu respetivo tipo
            var events = await _context.Events
                .Include(e => e.Type)
                .FirstOrDefaultAsync(m => m.Id == id);

            //Valida se evento existe
            if (events == null)
            {
                _logger.LogWarning("Event not found.");
                return NotFound();
            }

            ViewData["eventType"] = events.Type.Name;

            return View(events);
        }

        /// <summary>
        /// Cria
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Executing DeleteConfirmed POST action of EventsController.");

            if (_context.Events == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Events'  is null.");
            }
            //Remove o evento da BD
            var events = await _context.Events.FindAsync(id);
            if (events != null)
            {
                _context.Events.Remove(events);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Valida se evento existe
        private bool EventsExists(int id)
        {
            return (_context.Events?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> AddOwner(int Id, string Email)
        {
            _logger.LogWarning("A adicionar novo owner ao evento - checking event");
            var events = await _context.Events.FindAsync(Id);
            if (events == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }
            _logger.LogWarning("A adicionar novo owner ao evento - checking user by email");
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogWarning("A adicionar novo owner ao evento - adding");
                user.ListaEventos.Add(events);
                events.ListaUtilizadores.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "New owner Added";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
                {
                    // Chave duplicada
                    _logger.LogError("Chave duplicada ao adicionar owner ao evento.");
                    TempData["ErrorMessage"] = "Already Owned";
                }
                else
                {
                    // Outro erro de atualização do banco de dados
                    _logger.LogError("Erro ao adicionar owner ao evento: " + ex.Message);
                    TempData["ErrorMessage"] = "Error adding new owner.";
                }

                return RedirectToAction(nameof(Index));
            }
   
        }
    }
}
