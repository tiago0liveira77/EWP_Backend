using EWP_API_WEB_APP.Data;
using EWP_API_WEB_APP.Models.API.Requests;
using EWP_API_WEB_APP.Models.Data;
using EWP_API_WEB_APP.Utilities.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace EWP_API_WEB_APP.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuthController(ILogger<AuthController> logger, ApplicationDbContext dbContext, UserManager<Users> userManager,
            SignInManager<Users> signInManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Carrega a página de login
        /// </summary>
        /// <param name="InvalidLogin"> Serve para mostrar erro ao executar login</param>
        /// <returns></returns>
        public ActionResult Index(bool InvalidLogin)
        {
            if (InvalidLogin != null)
                ViewBag.InvalidLogin = InvalidLogin;

            _logger.LogInformation("AuthController: Index action executed.");
            return View();
        }

        /// <summary>
        /// Efetua o login do utilizador
        /// </summary>
        /// <param name="loginRequest">Dados de request para o login (email e password)</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            Users user = null;

            // Valida o email
            if (loginRequest.email == null)
            {
                ViewBag.InvalidLogin = true;
                _logger.LogInformation("AuthController: Login - Invalid email provided.");
            }
            else
            {
                // Obtem o user
                user = await _userManager.FindByEmailAsync(loginRequest.email);
            }

            // Valida se o utilizador existe
            if (user != null)
            {
                // Verifica se a password está correta
                PasswordVerificationResult passWorks = new PasswordHasher<Users>().VerifyHashedPassword(null, user.PasswordHash, loginRequest.password);

                if (passWorks.Equals(PasswordVerificationResult.Success))
                {
                    // Login efetuado
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Guarda em sessão o nome do utilizador para o mostrar na NavBar
                    HttpContext.Session.SetString("UserName", user.Name);

                    ViewBag.InvalidLogin = false;
                    _logger.LogInformation("AuthController: Login - Successful login.");
                    return RedirectToAction("Index", "Home", new { InvalidLogin = false });
                }
            }

            // Login inválido
            TempData["userName"] = null;
            ViewBag.InvalidLogin = true;
            _logger.LogInformation("AuthController: Login - Invalid login attempt.");
            return RedirectToAction("Index", "Auth", new { InvalidLogin = true });
        }

        /// <summary>
        /// Carrega a página de registar uma nova conta de um cliente
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            var model = new Users();
            _logger.LogInformation("AuthController: Create action executed.");
            return View(model);
        }

        /// <summary>
        /// Cria um novo utilizador na base de dados
        /// </summary>
        /// <param name="model"> modelo da tabela de utilizadores </param>
        /// <param name="password"> password do utilizador </param>
        /// <param name="confirmPassword"> password de confirmação </param>
        /// <param name="roleChoosed"> o tipo de conta de utilizador</param>
        /// <param name="image"> imagem de perfil do utilizador</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Users model, string password, string confirmPassword, string roleChoosed, IFormFile image)
        {
            // Não validar o roleChoosed e a imagem
            ModelState.Remove(roleChoosed);
            ModelState.Remove(nameof(image));

            // Verifica o username inserido
            if (string.IsNullOrEmpty(model.UserName))
            {
                ModelState.AddModelError("UserName", "UserName is required.");
                _logger.LogInformation("AuthController: Create - UserName is required.");
            }
            else if (model.Name.Length < 3)
            {
                ModelState.AddModelError("UserName", "Your UserName must be at least 3 characters");
                _logger.LogInformation("AuthController: Create - UserName must be at least 3 characters.");
            }

            // Valida primeiramente os campos indicados no model Users
            if (ModelState.IsValid)
            {
                // Cria o objeto do utilizador
                var user = new Users
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Status = 1
                };
                user.CreationDate = DateTime.Now;
                user.LastLoginDate = DateTime.Now;

                // Validar se as passwords dão match
                if (password.Equals(confirmPassword))
                {
                    // Criado o path para dar upload
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/uploadedProfileImages");
                    // Upload do ficheiro
                    user.image = FileUtils.addImageToServer(image, _logger, uploadPath);
                    // Criar o utilizador
                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        // Se foi criado com sucesso, associar ao utilizador o seu ROLE na app.
                        if (!await _roleManager.RoleExistsAsync(roleChoosed))
                        {
                            var role = new IdentityRole(roleChoosed);
                            await _roleManager.CreateAsync(role);
                        }
                        await _userManager.AddToRoleAsync(user, roleChoosed);
                        // Realizar o login do cliente acabado de criar
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        // Guardar em sessão para mostrar o nome na NAVBAR
                        HttpContext.Session.SetString("UserName", user.Name);

                        _logger.LogInformation("AuthController: Create - User created successfully.");
                        return RedirectToAction("Index", "Home"); // Página principal
                    }
                    else
                    {
                        // Eliminar imagem caso tenha sido criada
                        string caminhoImagem = Path.Combine(uploadPath, user.image);
                        if (System.IO.File.Exists(caminhoImagem))
                        {
                            System.IO.File.Delete(caminhoImagem);
                        }
                        ViewData["errorCreatingUser"] = result.Errors.First().Description;

                        _logger.LogInformation("AuthController: Create - Error creating user: " + result.Errors.First().Description);
                    }
                }
                else
                {
                    ViewData["errorCreatingUser"] = "Passwords don't match.";
                    _logger.LogInformation("AuthController: Create - Passwords don't match.");
                }
            }
            else
            {
                ViewData["errorCreatingUser"] = "There are Fields with invalid values";
                _logger.LogInformation("AuthController: Create - Invalid values in the fields.");
            }
            return View(model);
        }


        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            _logger.LogInformation("AuthController: Logout - User logged out.");

            return RedirectToAction("Index", "Home");
        }
    }
}
