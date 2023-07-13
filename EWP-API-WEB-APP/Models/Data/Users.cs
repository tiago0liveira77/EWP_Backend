using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EWP_API_WEB_APP.Models.Data
{
    /// <summary>
    /// Dados dos utilizadores
    /// </summary>
    public class Users : IdentityUser
    {
        public Users()
        {
            ListaEventos = new HashSet<Events>();
            ListaBilhetes = new HashSet<Tickets>();
        }

        /// <summary>
        /// Nome do utilizador
        /// </summary>
        [Required(ErrorMessage = "You must insert a valid name.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Your name must be at least 3 characters!")]
        public string Name { get; set; }

        /// <summary>
        /// Data da criação da conta
        /// </summary>
        [Display(Name = "Creation Date")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Data de ultimo login
        /// </summary>
        [Display(Name = "Last Login Date")]
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Estado da conta do utilizador
        /// </summary>
        [Display(Name = "State")]
        public int Status { get; set; }

        /// <summary>
        /// Nome da imagem de perfil do utilizador
        /// </summary>
        public string image { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "Phone Number must be at 9 characters length!")]
        [RegularExpression("9[1236][0-9]{7}", ErrorMessage = "Phone Number is not valid")]
        public override string PhoneNumber { get; set; }

        //******************************************************
        // CRIAÇÃO DE CHAVE ESTRANGEIRAS
        //******************************************************

        /// <summary>
        /// Lista de eventos que um utilizador vai
        /// </summary>
        public ICollection<Events> ListaEventos { get; set; }

        /// <summary>
        /// Lista de bilhetes que um utilizador possui
        /// </summary>
        public ICollection<Tickets> ListaBilhetes { get; set; }

    }

}
