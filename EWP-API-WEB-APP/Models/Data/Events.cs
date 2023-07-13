using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EWP_API_WEB_APP.Utilities.Validations;

namespace EWP_API_WEB_APP.Models.Data
{

    /// <summary>
    /// Dados dos eventos
    /// </summary>
    public class Events
    {

        public Events()
        {
            ListaBilhetes = new HashSet<Tickets>();
            ListaUtilizadores = new HashSet<Users>();
        }

        /// <summary>
        /// PK
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome do evento
        /// </summary>
        [Required(ErrorMessage = "You must insert a valid name!")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Your name must be at least 3 characters!")]
        public string Name { get; set; }

        /// <summary>
        /// Descrição do evento
        /// </summary>
        [Required(ErrorMessage = "You must insert a valid description!")]
        [StringLength(300, MinimumLength = 1, ErrorMessage = "Your description must be at least 1 character!")]
        public string Description { get; set; }

        /// <summary>
        /// Data de inicio do evento
        /// </summary>
        [Display(Name = "Starting date")]
        [Required(ErrorMessage = "You must select a valid starting date!")]
        [EventsDatesValidation("StartingDate", "EndingDate", ErrorMessage = "The starting date must be before the ending date.")]
        [FutureDateValidation("StartingDate", ErrorMessage = "The starting date must be on the future.")]
        public DateTime StartingDate { get; set; }

        /// <summary>
        /// Data de fim do evento
        /// </summary>
        [Display(Name = "Ending date")]
        [Required(ErrorMessage = "You must select a valid ending date!")]
        [FutureDateValidation("EndingDate", ErrorMessage = "The ending date must be on the future.")]
        public DateTime EndingDate { get; set; }

        /// <summary>
        /// Localização do evento
        /// </summary>
        [Required(ErrorMessage = "You must insert a valid location")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Your location must be at least 3 characters!")]
        public string Location { get; set; }

        /// <summary>
        /// Nome da foto do evento
        /// </summary>
        [Display(Name = "File")]
        public string Ficheiro { get; set; }

        /// <summary>
        /// Tipo de evento
        /// </summary>
        [Required(ErrorMessage = "You must select a valid event type")]
        [Display(Name = "Event type")]
        public EventsType Type { get; set; }

        //******************************************************
        // CRIAÇÃO DE CHAVE ESTRANGEIRAS
        //******************************************************

        /// <summary>
        /// Lista de bilhetes associados a um evento
        /// </summary>
        public ICollection<Tickets> ListaBilhetes { get; set; }

        /// <summary>
        /// Lista de utilizadores associados a um evento
        /// </summary>
        public ICollection<Users> ListaUtilizadores { get; set; }
    }
}
