using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWP_API_WEB_APP.Models.Data
{

    /// <summary>
    /// Dados sobre as vendas dos bilhetes e quem as comprou
    /// </summary>
    public class TicketSales
    {
        /// <summary>
        /// PK
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data de compra do bilhete
        /// </summary>
        public DateTime PurchaseDate { get; set; }

        //******************************************************
        // CRIAÇÃO DE CHAVE ESTRANGEIRAS
        //******************************************************

        /// <summary>
        /// FK para o bilhete a quem a venda está associada
        /// </summary>
        [ForeignKey(nameof(Ticket))]
        public int TicketFK { get; set; }
        public Tickets Ticket { get; set; }

        /// <summary>
        /// FK para o utilizador a quem a venda está associada
        /// </summary>
        [ForeignKey(nameof(User))]
        public String UserFK { get; set; }
        public Users User { get; set; }
    }
}
