using EWP_API_WEB_APP.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using static System.Net.WebRequestMethods;
using EWP_API_WEB_APP.Utilities.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace EWP_API_WEB_APP.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(UserManager<Users> userManager, ILogger<UserController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Página de perfil
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("UserController: Index");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("UserController: User not found, redirecting to Login");
                // Se o utilizador não for encontrado
                return RedirectToAction("Login", "Account");
            }

            ViewData["image"] = user.image;

            return View(user);
        }

        /// <summary>
        /// Página de edição de utilizador
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            _logger.LogInformation("UserController: Edit");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("UserController: User not found, redirecting to Login");
                // Se o utilizador não for encontrado
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        /// <summary>
        /// Ação de editar o utilizador
        /// </summary>
        /// <param name="model"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Name,Email,PhoneNumber")] Users model, IFormFile image)
        {
            _logger.LogInformation("UserController: Edit [HttpPost]");
            //Retira a imagem das validações
            ModelState.Remove(nameof(image));

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("UserController: User not found, redirecting to Login");
                    // Se o utilizador não for encontrado
                    return RedirectToAction("Login", "Account");
                }

                // Atualiza a informação do utilizador
                user.Name = model.Name;
                if (user.Email != model.Email)
                {
                    //Se o cliente alterar o email, o mesmo precisa de voltar a ser confirmado
                    user.EmailConfirmed = false;
                    user.Status = 1;
                }
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                if (image != null)
                {
                    //Criado o path para dar upload
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/uploadedProfileImages");
                    //Upload do ficheiro
                    user.image = FileUtils.addImageToServer(image, _logger, uploadPath);
                }

                //Atualizar utilizador
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("UserController: Edit - User updated successfully");
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
            {
                _logger.LogWarning("UserController: Edit - Model state is not valid");
                ViewData["errorCreatingUser"] = "There are Fields with invalid values";
            }

            return View(model);
        }

        /// <summary>
        /// Página de validação de OTP
        /// </summary>
        /// <param name="validationStatus"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Validate(int validationStatus)
        {
            _logger.LogInformation("UserController: Validate");
            ViewData["disabled"] = true;
            if (validationStatus != null)
            {
                ViewBag.ValidationStatus = validationStatus;
            }
            return View();
        }

        /// <summary>
        /// Método que envia o email com o OTP para o cliente
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendEmail()
        {
            _logger.LogInformation("UserController: SendEmail [HttpPost]");
            var user = await _userManager.GetUserAsync(User);

            _logger.LogWarning("sending code");
            //Gera o código
            Random r = new Random();
            int code = r.Next(100000, 999999);
            string remetenteEmail = "eventwalletpassport@gmail.com";
            string remetenteSenha = "ewsswybdaplwjewh";
            string destinatarioEmail = user.Email;
            string assunto = "Account Validation";
            // HTML template
            string corpo = @"<!DOCTYPE html>
                     <html>
                     <head>
                         <meta charset='UTF-8'>
                         <title>OTP Email</title>
                     </head>
                     <body>
                         <h2>One-Time Password (OTP) Email</h2>
                         <p>Dear Client,</p>
                         <p>Please find your One-Time Password (OTP) below:</p>
                         <h3 style='font-weight: bold;'>{{otp}}</h3>
                         <p>If you did not request this OTP, please ignore this email.</p>
                         <p>Thank you!</p>
                     </body>
                     </html>";

            // Troca o {{otp}} com o código para o utilizador
            corpo = corpo.Replace("{{otp}}", code.ToString());

            TempData["OTP"] = code;

            //Contruir objetos de envio de email
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(remetenteEmail, remetenteSenha);
            smtpClient.EnableSsl = true;

            MailMessage mailMessage = new MailMessage(remetenteEmail, destinatarioEmail, assunto, corpo);
            mailMessage.IsBodyHtml = true;

            //Enviar email
            smtpClient.Send(mailMessage);

            ViewData["disabled"] = false;
            return RedirectToAction("Validate", new { validationStatus = 3 });
        }


        /// <summary>
        /// Valida o OTP inserido pelo cliente
        /// </summary>
        /// <param name="OTPForm"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidateOTP(int OTPForm)
        {
            _logger.LogInformation("UserController: ValidateOTP [HttpPost]");
            int OTPLegit = 0;

            //OTPLegit é o otp gerado, verifica se foi bem guardado
            if (TempData["OTP"] != null)
            {
                OTPLegit = int.Parse(TempData["OTP"].ToString());
            }


            // Lógica para validar o OTP recebido
            _logger.LogWarning("OPT FROM: " + OTPForm);
            if (OTPLegit == 0)
            {
                //erro token
                _logger.LogWarning("OTPLegit null");
                return RedirectToAction("Validate", new { validationStatus = -1 });
            }
            else
            {
                if (OTPForm == OTPLegit)
                {
                    //conta validada
                    _logger.LogWarning("Tokens Validos");

                    _logger.LogWarning("Editing status to active user");
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        _logger.LogWarning("UserController: User not found, redirecting to Login");
                        // Se o utilizador não for encontrado
                        return RedirectToAction("Login", "Account");
                    }

                    // Atualizar informação do utilizador
                    user.Status = 2;
                    user.EmailConfirmed = true;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("UserController: ValidateOTP - User updated successfully");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }

                }
                else
                {
                    //token inserido diferente do token enviado
                    _logger.LogWarning("Tokens diferentes: " + OTPLegit + " != " + OTPForm);
                    return RedirectToAction("Validate", new { validationStatus = 2 });
                }
            }
        }
    }
}
