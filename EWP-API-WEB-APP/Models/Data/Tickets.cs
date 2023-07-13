using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWP_API_WEB_APP.Models.Data
{
    /// <summary>
    /// Dados dos bilhetes
    /// </summary>
    public class Tickets
    {

        public Tickets()
        {
            ListaUtilizadores = new HashSet<Users>();
            ListaPulseiras = new HashSet<Wristbands>();
        }
        /// <summary>
        /// PK
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome do bilhete
        /// </summary>
        [Display(Name = "Ticket name")]
        [Required(ErrorMessage = "You must insert a valid ticket name!")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Your ticket name must be at least 3 characters!")]
        public string Nome { get; set; }

        /// <summary>
        /// Descrição do bilhete (beneficios)
        /// </summary>
        [Required(ErrorMessage = "You must insert a valid ticket description!")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Your ticket description must be at least 3 characters!")]
        public string Description { get; set; }

        /// <summary>
        /// Preço do bilhete
        /// </summary>
        [Display(Name = "Ticket price")]
        [Range(0, double.MaxValue, ErrorMessage = "The price can't be a negative value!")]
        public float Price { get; set; }

        /// <summary>
        /// Quantidade disponivel para venda
        /// </summary>
        [Display(Name = "Available")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser um valor não negativo.")]
        public int Selling { get; set; }

        //******************************************************
        // CRIAÇÃO DE CHAVE ESTRANGEIRAS
        //******************************************************

        /// <summary>
        /// FK para o evento a quem o bilhete está associado
        /// </summary>
        [ForeignKey(nameof(Event))]
        public int EventFK { get; set; }
        public Events? Event { get; set; }

        /// <summary>
        /// Lista de utilizadores que possuem o bilhete referenciado associado
        /// </summary>
        public ICollection<Users>? ListaUtilizadores { get; set; }

        /// <summary>
        /// Lista de pulseiras que possuem o bilhete referenciado associado 
        /// </summary>
        public ICollection<Wristbands>? ListaPulseiras { get; set; }
    }
}
