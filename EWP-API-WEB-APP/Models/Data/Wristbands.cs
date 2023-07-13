using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWP_API_WEB_APP.Models.Data
{
    /// <summary>
    /// Estados possíveis para a pulseira do utilizador
    /// </summary>
    public enum WristbandsStatus
    {
        Active = 1,
        Inactive = 2,
        Lost = 3,
        Error = 4
    }

    /// <summary>
    /// Dados das pulseiras
    /// </summary>
    public class Wristbands
    {
        public Wristbands()
        {
            ListaBilhetes = new HashSet<Tickets>();
        }

        /// <summary>
        /// PK
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Código NFC identificador da pulseira
        /// </summary>
        public string CodeNFC { get; set; }

        /// <summary>
        /// Cor da pulseira
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Data que a pulseira for ativada
        /// </summary>
        public DateTime ConnectedDate { get; set; }

        /// <summary>
        /// Marca da pulseira
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Estado da pulseira
        /// </summary>
        public WristbandsStatus Status { get; set; }

        //******************************************************
        // CRIAÇÃO DE CHAVE ESTRANGEIRAS
        //******************************************************

        /// <summary>
        /// FK para o utilizador a quem a pulseira está associada
        /// </summary>
        [ForeignKey(nameof(User))]
        public String UserFK { get; set; }
        public Users User { get; set; }

        /// <summary>
        /// Lista de bilhetes associados a uma pulseira
        /// </summary>
        public ICollection<Tickets> ListaBilhetes { get; set; }

    }
}
