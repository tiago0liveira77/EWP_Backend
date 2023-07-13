using System.ComponentModel.DataAnnotations;

namespace EWP_API_WEB_APP.Models.Data
{
    public class EventsType
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Event Type")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
