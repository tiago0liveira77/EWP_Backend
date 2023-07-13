using Azure.Core;
using EWP_API_WEB_APP.Data;
using EWP_API_WEB_APP.Models.API.Requests;
using EWP_API_WEB_APP.Models.API.Responses;
using EWP_API_WEB_APP.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Sockets;


namespace EWP_API_WEB_APP.Controllers.API
{

    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<DataController> _logger;

        public DataController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<DataController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        //  GET: api/data/wristbands?id=1&nome=Tiago

        [HttpGet]
        [Route("wristbands")]
        public List<Wristbands> GetAllWirstBands()
        {


            var allWristbands = _context.Wristbands.ToList();


            return allWristbands;
        }




        // GET: api/data/users
        [HttpGet]
        [Route("users")]
        public List<Users> GetAllUsers()
        {
            var allUsers = _context.Users.ToList();

            return allUsers;
        }

        // GET: api/data/bands
        [HttpGet]
        [Route("bands")]
        public List<Wristbands> GetAllBands()
        {
            var allWristbands = _context.Wristbands.ToList();

            return allWristbands;
        }

        // GET: api/data/events
        [HttpGet]
        [Route("events")]
        public List<Events> GetAllEvents()
        {
            List<Events> allEvents = _context.Events.Include(e => e.Type).ToList();

            foreach(Events evt in allEvents){
                
            }

            _logger.LogWarning("api/Data/events: returning list of all events");
            return allEvents;
        }



        // GET: api/data/getTickets
        [HttpGet]
        [Route("getTickets")]
        public List<Tickets> getTicketsByEvent(int id)
        {

            // build the query using LINQ
            var query = from t in _context.Tickets
                        where t.EventFK == id
                        select t;

            // execute the query
            var result = query.ToList();

            return result;

        }

        // GET: api/data/getUserTickets
        [HttpGet]
        [Route("getUserTickets")]
        public List<Tickets> GetUserTickets(string email)
        {
            // Retrieve the user based on the email
            Users user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                // User not found, return an empty list
                return new List<Tickets>();
            }

            // Retrieve the tickets associated with the user and include the event details
            List<Tickets> tickets = _context.Tickets
                .Where(t => t.ListaUtilizadores.Contains(user))
                .Select(t => new Tickets
                {
                    Id = t.Id,
                    Nome = t.Nome,
                    Description = t.Description,
                    Price = t.Price,
                    Selling = t.Selling,
                    Event = new Events
                    {
                        Name = t.Event.Name // Retrieve the event name
                                            // Set other properties of Events if needed
                    }
                })
                .ToList();

            return tickets;
        }



        // GET: api/data/getTicketsDetails
        [HttpGet]
        [Route("getTicketsDetails")]
        public List<Tickets> getTicketsDetails(int id)
        {

            // build the query using LINQ
            var query = from t in _context.Tickets
                        where t.Id == id
                        select t;

            // execute the query
            var result = query.ToList();

            return result;

        }


        [HttpPost]
        [Route("changeProfile")]
        public IActionResult ChangeProfileDetails(ChangeProfileRequest request)
        {
            // Retrieve the user from the database based on the user's ID or any other identifier
            var user = _context.Users.FirstOrDefault(u => u.Email == request.email);

            if (user == null)
            {
                return NotFound(); // User not found in the database
            }

            // Update the user's profile information with the received values
            user.Name = request.name;
            user.Email = request.email;
            //user.Cellphone = request.cellphone;

            // Save the changes to the database
            _context.SaveChanges();

            return Ok(user); // Profile updated successfully

        }

        [HttpPost]
        [Route("buyTicket")]
        public async Task<IActionResult> BuyTicket(BuyTicketRequest request)
        {
            Users user = _context.Users.FirstOrDefault(u => u.Email == request.email);

            Tickets ticket = _context.Tickets.FirstOrDefault(u => u.Id == request.ticketID);

            user.ListaBilhetes.Add(ticket);
            _context.SaveChanges();

            return Ok("SUCCESS: ticketId: " + ticket.Id + ", email: " + user.Email);
        }

        [HttpPost]
        [Route("addevent")]
        public async Task<IActionResult> Upload([FromForm] string nome, [FromForm] string descricao, [FromForm] DateTime startingDate, [FromForm] DateTime endingDate, [FromForm] string location, [FromForm] IFormFile? imagem, [FromForm] int eventype, [FromForm] string email)
        {
            _logger.LogWarning("New Event - Adding New Event: " + nome);
            string imageFileName = "default-image.jpg";
            if (imagem != null && imagem.Length != 0)
            {
                //return BadRequest("Nenhum arquivo de imagem enviado.");
                // Verificar a extensão do arquivo
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imagem.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogError("New Event - ERROR: Event: " + nome + " - FILE IS NOT AN IMAGE");
                    return BadRequest("Apenas imagens são permitidas.");
                }
                try
                {
                    DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
                    long timestampSeconds = dateTimeOffset.ToUnixTimeSeconds();
                    imageFileName = timestampSeconds.ToString() + fileExtension;
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    string filePath = Path.Combine(uploadPath, imageFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        imagem.CopyTo(fileStream);
                    }



                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao fazer upload do arquivo de imagem: {ex.Message}");
                }
            }

            EventsType type = _context.EventsType.FirstOrDefault(u => u.Id == eventype);

            var newEvent = new Events
            {
                Name = nome,
                Description = descricao,
                StartingDate = startingDate,
                EndingDate = endingDate,
                Location = location,
                Ficheiro = imageFileName,
                Type = type
            };

            // Retrieve the user who created the event
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                // Handle the case where the user is not found
                return NotFound();
            }

            _context.Events.Add(newEvent);
            user.ListaEventos.Add(newEvent);
            _context.SaveChanges();

            _logger.LogWarning("New Event - SUCCESS - Event: " + newEvent.Name + "Ficheiro: " + imageFileName);

            //return Ok("SUCESSO: Nome: " + newEvent.Name + ", Desc: " + newEvent.Description + ", startDate: " + newEvent.StartingDate + ", endDate: " + newEvent.EndingDate + ", Local: " + newEvent.Location + " Imagem: " + imageFileName + ", Type: " + newEvent.Type);
            return Ok("Event Creation Success");


        }

        // GET: api/data/getUserEvents
        [HttpGet]
        [Route("getUserEvents")]
        public List<Events> GetUserEvents(string email)
        {
            // Retrieve the user based on the email
            Users user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                // User not found, return an empty list
                return new List<Events>();
            }

            // Retrieve the tickets associated with the user and include the event details
            List<Events> events = _context.Events
                .Where(t => t.ListaUtilizadores.Contains(user))
                .Select(t => new Events
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    StartingDate = t.StartingDate,
                    EndingDate = t.EndingDate,
                    Location = t.Location,
                    Ficheiro = t.Ficheiro,
                    Type = t.Type
                })
                .ToList();

            return events;
        }

        [HttpPost]
        [Route("changeEvent")]
        public async Task<IActionResult> ChangeEvent([FromForm] int eventId, [FromForm] string nome, [FromForm] string descricao, [FromForm] DateTime startingDate, [FromForm] DateTime endingDate, [FromForm] string location, [FromForm] IFormFile? imagem, [FromForm] int eventype)
        {
            _logger.LogWarning("New Event - Editing Event: " + nome);

            var eventData = _context.Events.FirstOrDefault(e => e.Id == eventId);

            if (eventData == null)
            {
                return NotFound(); // Event not found in the database
            }

            string imageFileName = eventData.Ficheiro;
            if (imagem != null && imagem.Length != 0)
            {
                //return BadRequest("Nenhum arquivo de imagem enviado.");
                // Verificar a extensão do arquivo
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imagem.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogError("Editing Event - ERROR: Event: " + nome + " - FILE IS NOT AN IMAGE");
                    return BadRequest("Apenas imagens são permitidas.");
                }
                try
                {
                    DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
                    long timestampSeconds = dateTimeOffset.ToUnixTimeSeconds();
                    imageFileName = timestampSeconds.ToString() + fileExtension;
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    string filePath = Path.Combine(uploadPath, imageFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        imagem.CopyTo(fileStream);
                    }



                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao fazer upload do arquivo de imagem: {ex.Message}");
                }
            }

            EventsType type = _context.EventsType.FirstOrDefault(u => u.Id == eventype);

            eventData.Name = nome;
            eventData.Description = descricao;
            eventData.StartingDate = startingDate;
            eventData.EndingDate = endingDate;
            eventData.Location = location;
            eventData.Type = type;
            eventData.Ficheiro = imageFileName;

            _context.SaveChanges();

            _logger.LogWarning("Edited Event - SUCCESS - Event: " + eventData.Name + "Ficheiro: " + imageFileName);

            //return Ok("SUCESSO: Nome: " + newEvent.Name + ", Desc: " + newEvent.Description + ", startDate: " + newEvent.StartingDate + ", endDate: " + newEvent.EndingDate + ", Local: " + newEvent.Location + " Imagem: " + imageFileName + ", Type: " + newEvent.Type);
            return Ok("Event Edited Success");


        }


        //vale apena fazer verificação de event owner para dar delete?
        [HttpDelete]
        [Route("deleteevent/{id}")]
        public IActionResult DeleteEvent(int id, int email)
        {
            var eventToDelete = _context.Events.FirstOrDefault(e => e.Id == id);

            if (eventToDelete == null)
            {
                _logger.LogError($"Event ID: {id} - not found.");
                return NotFound(); // Evento não encontrado
            }

            _context.Events.Remove(eventToDelete);
            _context.SaveChanges();

            _logger.LogWarning($"Delete Event ID: {id} - removed.");

            return Ok(); // Sucesso na remoção do evento
        }


        // GET: api/data/tickets
        [HttpGet]
        [Route("GetTicketsToManage")]
        public List<Tickets> GetTicketsToManage(int id)
        {
            var tickets = _context.Tickets.Where(t => t.EventFK == id).ToList();

            return tickets;
        }

        //
        [HttpPost]
        [Route("NewTicket")]
        public async Task<IActionResult> NewTicket([FromBody]Tickets newTicket)
        {
            _logger.LogWarning("New Ticket - Adding New Ticket: " + newTicket.Nome);

            var eventData = _context.Events.FirstOrDefault(e => e.Id == newTicket.EventFK);

            if (eventData == null)
            {
                return NotFound(); // Event not found in the database
            }

            _context.Tickets.Add(newTicket);
            _context.SaveChanges();

           
            return Ok("New ticket added");
        }

        //
        [HttpPost]
        [Route("EditTicket")]
        public async Task<IActionResult> EditTicket([FromBody] Tickets newTicket)
        {
            _logger.LogWarning("EditTicket - Editting New Ticket: " + newTicket.Nome);

            var ticketData = _context.Tickets.FirstOrDefault(e => e.Id == newTicket.Id);

            if (ticketData == null)
            {
                return NotFound(); // Event not found in the database
            }


            ticketData.Nome = newTicket.Nome;
            ticketData.Description = newTicket.Description;
            ticketData.Price = newTicket.Price;
            ticketData.Selling  = newTicket.Selling;

            _logger.LogWarning("EditTicket - Saving Edit");
            _context.SaveChanges();


            return Ok("New ticket added");
        }


        [HttpDelete]
        [Route("deleteticket/{id}")]
        public IActionResult DeleteTicket(int id)
        {
            var ticketToDelete = _context.Tickets.FirstOrDefault(e => e.Id == id);

            if (ticketToDelete == null)
            {
                _logger.LogError($"Ticket ID: {id} - not found.");
                return NotFound(); // Evento não encontrado
            }

            _context.Tickets.Remove(ticketToDelete);
            _context.SaveChanges();

            _logger.LogWarning($"Delete Ticket ID: {id} - removed.");

            return Ok(); // Sucesso na remoção do evento
        }

    }
}
